using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Refs")]
    public BoardRuntime runtime;
    public TurnManager turns;
    public PersonalityProfile profile;

    [Header("Lookahead")]
    public bool   useLookahead = true;
    [Range(0f, 1f)] public float lookaheadDecay = 0.5f; // weight for future step
    public int    maxFollowupMovesScored = 32;          // cap for safety

    Coroutine _turnRoutine;
    Piece _lastMovedThisTurn;

    void Awake()
    {
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!turns)   turns   = TurnManager.Instance;
    }

    void OnEnable()
    {
        if (turns != null) turns.OnTurnStarted += HandleTurnStarted;
    }

    void OnDisable()
    {
        if (turns != null) turns.OnTurnStarted -= HandleTurnStarted;
        if (_turnRoutine != null) StopCoroutine(_turnRoutine);
    }

    void HandleTurnStarted(Team team, int energy, int max)
    {
        if (profile == null) { Debug.LogWarning("AIController: No PersonalityProfile set."); return; }
        if (team != profile.team) return;

        if (_turnRoutine != null) StopCoroutine(_turnRoutine);
        _turnRoutine = StartCoroutine(RunTurn());
    }

    IEnumerator RunTurn()
    {
        _lastMovedThisTurn = null;
        int actions = 0;

        while (turns.currentTeam == profile.team && turns.energy > 0 && actions < profile.maxActionsPerTurn)
        {
            var best = PickBestByPriority();
            if (best.piece == null) break;

            if (best.score < profile.minScoreToAct) break;

            // Telegraph intent
            yield return Telegraph(best.piece, best.option.dest);

            // Execute for real
            bool ok = MoveResolver.TryExecuteMove(best.piece, best.option.dest, best.option.cost, runtime);
            actions++;
            if (ok) _lastMovedThisTurn = best.piece;
            if (!ok) break;

            if (profile.actionDelay > 0f) yield return new WaitForSeconds(profile.actionDelay);
            else yield return null;
        }

        if (turns.currentTeam == profile.team)
            turns.EndTurn();
    }

    // ---------- Priority funnel ----------

    struct Candidate { public Piece piece; public MoveOption option; public float score; }

    Candidate PickBestByPriority()
    {
        Candidate best = default;
        best.score = float.NegativeInfinity;

        foreach (var prio in profile.priorities)
        {
            var set = CollectCandidatesForPriority(prio);
            foreach (var c in set)
            {
                float s = ScoreMoveLookahead(prio, c.piece, c.option);
                if (s > best.score) { best = c; best.score = s; }
            }
            if (best.piece != null) break; // we found something in this tier
        }

        return best;
    }

    List<Candidate> CollectCandidatesForPriority(PersonalityPriority prio)
    {
        var list = new List<Candidate>(64);

        foreach (var p in EnumeratePieces(profile.team))
        {
            var moves = MoveGenerator.GetMoves(p, runtime);
            foreach (var opt in moves)
            {
                if (!runtime.InBounds(opt.dest)) continue;
                if (turns.energy < opt.cost)     continue;

                var target = runtime.GetPiece(opt.dest);

                switch (prio)
                {
                    case PersonalityPriority.LethalCapture:
                        if (IsEnemy(target, p) && (target.HP - p.Attack) <= 0)
                            list.Add(new Candidate { piece = p, option = opt });
                        break;

                    case PersonalityPriority.Capture:
                        if (IsEnemy(target, p))
                            list.Add(new Candidate { piece = p, option = opt });
                        break;

                    case PersonalityPriority.Approach:
                        if (target == null)
                        {
                            var nearest = FindNearestEnemy(p.GridPos, p.Team);
                            if (nearest.HasValue)
                            {
                                int d0 = Manhattan(p.GridPos, nearest.Value);
                                int d1 = Manhattan(opt.dest,  nearest.Value);
                                if (d1 < d0)
                                    list.Add(new Candidate { piece = p, option = opt });
                            }
                        }
                        break;

                    case PersonalityPriority.Retreat:
                        if (target == null && ShouldRetreat(p))
                        {
                            var nearest = FindNearestEnemy(p.GridPos, p.Team);
                            int threatNow = CountThreatsAgainst(p.GridPos, p.Team);
                            int threatThen = CountThreatsAgainst(opt.dest, p.Team);

                            bool safer = threatThen < threatNow;
                            bool farther = nearest.HasValue && Manhattan(opt.dest, nearest.Value) > Manhattan(p.GridPos, nearest.Value);
                            if (safer || farther)
                                list.Add(new Candidate { piece = p, option = opt });
                        }
                        break;
                }
            }
        }
        return list;
    }

    // ---------- Scoring with 1-step lookahead ----------

    float ScoreMoveLookahead(PersonalityPriority prio, Piece attacker, MoveOption opt)
    {
        // direct score now
        float direct = ScoreMove(prio, attacker, opt);

        if (!useLookahead) return direct;

        int remainingEnergy = turns.energy - opt.cost;
        if (remainingEnergy <= 0) return direct;

        // Simulate the move on the board array only, then evaluate best immediate follow-up for THIS piece
        SimSnapshot snap;
        if (!BeginSimulation(attacker, opt, out snap))
            return direct;

        float futureBest = 0f;
        var followups = MoveGenerator.GetMoves(attacker, runtime);
        int counted = 0;
        foreach (var f in followups)
        {
            if (!runtime.InBounds(f.dest)) continue;
            if (remainingEnergy < f.cost)  continue;

            float s2 = ScoreMove(prio, attacker, f); // use same scoring for the second step
            if (s2 > futureBest) futureBest = s2;

            counted++;
            if (counted >= maxFollowupMovesScored) break;
        }

        EndSimulation(snap);

        return direct + lookaheadDecay * futureBest;
    }

    float ScoreMove(PersonalityPriority prio, Piece attacker, MoveOption opt)
    {
        float score = 0f;
        var from = attacker.GridPos;
        var to   = opt.dest;

        var target = runtime.GetPiece(to);

        // Capture / damage reward
        if (target != null && target.Team != attacker.Team)
        {
            int hpAfter = Mathf.Max(0, target.HP - attacker.Attack);
            if (hpAfter <= 0)
            {
                score += profile.wLethalCapture;
                score += GetPieceValue(target) * profile.wTargetValue;
            }
            else
            {
                int dmg = attacker.Attack;
                score += dmg * profile.wCaptureDamage;
                score += GetPieceValue(target) * profile.wTargetValue * 0.5f;
            }
        }
        else
        {
            // Approach reward
            var nearest = FindNearestEnemy(from, attacker.Team);
            if (nearest.HasValue)
            {
                int d0 = Manhattan(from, nearest.Value);
                int d1 = Manhattan(to,   nearest.Value);
                int delta = d0 - d1; // >0 if closer
                score += delta * profile.wCloseDistance;
            }

            // Stride per energy
            int stride = Chebyshev(from, to);
            float perEnergy = stride / Mathf.Max(1f, opt.cost);
            score += perEnergy * profile.wStride;
        }

        // Destination danger penalty
        int threats = CountThreatsAgainst(to, attacker.Team);
        score -= threats * profile.wThreatPenalty;

        // Energy cost penalty
        score -= opt.cost * profile.wEnergyCost;

        // Repeat penalty (avoid spamming same piece)
        if (_lastMovedThisTurn != null && attacker == _lastMovedThisTurn)
            score -= profile.wRepeat;

        // Noise
        if (profile.noiseStdDev > 0f)
            score += Random.Range(-profile.noiseStdDev, profile.noiseStdDev);

        return score;
    }

    // ---------- Lightweight board simulation (array-only) ----------

    struct SimSnapshot
    {
        public Piece piece;
        public Vector2Int from;
        public Vector2Int placedAt; // where we put the piece during sim (dest or approach)
        public Piece capturedAtDest; // if there was an enemy on dest, we keep reference to restore
    }

    bool BeginSimulation(Piece attacker, MoveOption opt, out SimSnapshot snapshot)
    {
        snapshot = default;

        if (runtime?.pieces == null) return false;

        var from = attacker.GridPos;
        var to   = opt.dest;
        var target = runtime.GetPiece(to);

        // Determine simulated placement:
        // - if capture and target survives → move to approach cell next to target
        // - else → move into dest (empty or lethal capture)
        Vector2Int placeAt = to;
        if (target != null && target.Team != attacker.Team)
        {
            int hpAfter = Mathf.Max(0, target.HP - attacker.Attack);
            if (hpAfter > 0)
            {
                var maybeApproach = ComputeApproachCell(from, to);
                if (maybeApproach.HasValue) placeAt = maybeApproach.Value;
            }
        }

        // Save snapshot
        snapshot.piece = attacker;
        snapshot.from = from;
        snapshot.placedAt = placeAt;
        snapshot.capturedAtDest = runtime.pieces[to.x, to.y];

        // Mutate array & GridPos ONLY (no transforms, no events)
        runtime.pieces[from.x, from.y] = null;

        // If we are sim-placing on dest and there was a target, pretend it is removed
        if (placeAt == to)
        {
            // remove target from the grid for the sim
            runtime.pieces[to.x, to.y] = null;
        }

        // Place attacker
        runtime.pieces[placeAt.x, placeAt.y] = attacker;
        attacker.GridPos = placeAt;

        return true;
    }

    void EndSimulation(SimSnapshot s)
    {
        if (runtime?.pieces == null) return;

        // Remove attacker from sim cell
        if (runtime.pieces[s.placedAt.x, s.placedAt.y] == s.piece)
            runtime.pieces[s.placedAt.x, s.placedAt.y] = null;

        // Restore captured target if any (only if we had removed it at dest)
        if (s.capturedAtDest != null)
        {
            var to = s.capturedAtDest.GridPos; // defender's GridPos didn't change
            runtime.pieces[to.x, to.y] = s.capturedAtDest;
        }

        // Put attacker back
        runtime.pieces[s.from.x, s.from.y] = s.piece;
        s.piece.GridPos = s.from;
    }

    // Approach helper: last free cell on the line from 'from' → 'target' (adjacent to target)
    Vector2Int? ComputeApproachCell(Vector2Int from, Vector2Int target)
    {
        int dx = target.x - from.x;
        int dy = target.y - from.y;
        bool orth = (dx == 0) || (dy == 0);
        bool diag = Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!orth && !diag) return null;

        int sx = Mathf.Clamp(dx, -1, 1);
        int sy = Mathf.Clamp(dy, -1, 1);

        var cur = new Vector2Int(from.x + sx, from.y + sy);
        Vector2Int? lastFree = null;

        while (cur != target)
        {
            if (!runtime.InBounds(cur)) return null;
            if (runtime.GetPiece(cur) != null) return null; // blocked before target
            lastFree = cur;
            cur = new Vector2Int(cur.x + sx, cur.y + sy);
        }
        return lastFree;
    }

    // ---------- Telegraph (reuses your tile highlights) ----------

    IEnumerator Telegraph(Piece piece, Vector2Int dest)
    {
        SafeSet(piece.GridPos, profile.originColor);
        foreach (var c in ComputePathStraightOrDiag(piece.GridPos, dest))
            SafeSet(c, profile.pathColor);
        SafeSet(dest, profile.destColor);

        yield return new WaitForSeconds(Mathf.Max(0.1f, profile.actionDelay * 0.6f));

        SafeClear(piece.GridPos);
        foreach (var c in ComputePathStraightOrDiag(piece.GridPos, dest))
            SafeClear(c);
        SafeClear(dest);
    }

    // ---------- Helpers ----------

    IEnumerable<Piece> EnumeratePieces(Team team)
    {
        if (runtime?.pieces == null) yield break;
        int cols = runtime.builder.cols;
        int rows = runtime.builder.rows;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                var p = runtime.pieces[x, y];
                if (p != null && p.Team == team) yield return p;
            }
    }

    IEnumerable<Piece> EnumerateEnemies(Team myTeam)
    {
        if (runtime?.pieces == null) yield break;
        int cols = runtime.builder.cols;
        int rows = runtime.builder.rows;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                var p = runtime.pieces[x, y];
                if (p != null && p.Team != myTeam) yield return p;
            }
    }

    bool IsEnemy(Piece target, Piece self) => target != null && target.Team != self.Team;

    int Manhattan(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    int Chebyshev(Vector2Int a, Vector2Int b) => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

    float GetPieceValue(Piece p) => p.Attack * 2f + p.MaxHP * 1f; // replace with a proper map later

    int CountThreatsAgainst(Vector2Int cell, Team victimTeam)
    {
        int count = 0;
        foreach (var enemy in EnumerateEnemies(victimTeam))
        {
            var moves = MoveGenerator.GetMoves(enemy, runtime);
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].dest == cell) { count++; break; }
            }
        }
        return count;
    }

    IEnumerable<Vector2Int> ComputePathStraightOrDiag(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;
        bool orth = (dx == 0) || (dy == 0);
        bool diag = Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!orth && !diag) yield break;

        int sx = Mathf.Clamp(dx, -1, 1);
        int sy = Mathf.Clamp(dy, -1, 1);
        var cur = new Vector2Int(from.x + sx, from.y + sy);
        while (cur != to)
        {
            if (!runtime.InBounds(cur)) yield break;
            yield return cur;
            cur = new Vector2Int(cur.x + sx, cur.y + sy);
        }
    }

    void SafeSet(Vector2Int c, Color col)
    {
        if (runtime?.builder?.tiles == null) return;
        if (!runtime.InBounds(c)) return;
        runtime.builder.tiles[c.x, c.y]?.SetHighlight(col);
    }

    void SafeClear(Vector2Int c)
    {
        if (runtime?.builder?.tiles == null) return;
        if (!runtime.InBounds(c)) return;
        runtime.builder.tiles[c.x, c.y]?.ClearHighlight();
    }

    Vector2Int? FindNearestEnemy(Vector2Int from, Team myTeam)
    {
        Vector2Int? best = null;
        int bestDist = int.MaxValue;

        if (runtime?.pieces == null) return null;
        int cols = runtime.builder.cols;
        int rows = runtime.builder.rows;

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                var p = runtime.pieces[x, y];
                if (p == null || p.Team == myTeam) continue;
                int d = Manhattan(from, p.GridPos);
                if (d < bestDist) { bestDist = d; best = p.GridPos; }
            }
        return best;
    }

    bool ShouldRetreat(Piece p)
        {
            float hpPct = p.HP / Mathf.Max(1f, (float)p.MaxHP);
            int threats = CountThreatsAgainst(p.GridPos, p.Team);
            return hpPct <= profile.lowHPPercent || threats >= profile.threatRetreatThreshold;
        }
}

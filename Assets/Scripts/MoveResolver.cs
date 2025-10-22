// MoveResolver.cs (replace TryExecuteMove body)
using UnityEngine;

public static class MoveResolver
{
    public static bool TryExecuteMove(Piece piece, Vector2Int dest, int cost, BoardRuntime board, bool spendEnergy = true)
    {
        if (piece == null || board == null) return false;

        GameSignals.RaiseMoveStarted(piece, dest);

        // Energy check & spend
        if (spendEnergy)
        {
            var tm = TurnManager.Instance;
            if (tm == null) { Debug.LogError("No TurnManager."); return false; }
            if (!tm.TrySpend(cost))
            {
                GameSignals.RaiseMoveFailedInsufficientEnergy(piece, dest, cost);
                return false;
            }
        }

        var from = piece.GridPos;
        var target = board.GetPiece(dest) as Piece;

       // ... inside MoveResolver.TryExecuteMove, after we've got 'from', 'target' etc.

    if (target != null && target.Team != piece.Team)
    {
        // Start combat
        GameSignals.RaiseAttackStarted(piece, dest);
        var outcome = CombatResolver.ResolveAttack(piece, target);

        if (outcome.targetDied)
        {
            // Defender dies → replace
            UnityEngine.Object.Destroy(target.gameObject);
            board.pieces[dest.x, dest.y] = null;

            FireTileEvents(piece, board, from, dest);
            board.MovePiece(piece, dest);

            GameSignals.RaisePieceMoved(piece, from, dest, cost);
            GameSignals.RaiseAttackResolved(piece, dest, true);
        }
        else
        {
            // Defender survives → move to approach cell (adjacent to target along the line)
            var approach = ComputeApproachCell(board, from, dest);

            // If approach is valid and free and different from 'from', glide there
            if (approach.HasValue && approach.Value != from && board.GetPiece(approach.Value) == null)
            {
                FireTileEvents(piece, board, from, approach.Value);
                board.MovePiece(piece, approach.Value);
                GameSignals.RaisePieceMoved(piece, from, approach.Value, cost);
            }

            GameSignals.RaiseAttackResolved(piece, dest, false);
        }

        return true;
    }

        else if (target == null)
        {
            // Plain move (no combat)
            FireTileEvents(piece, board, from, dest);
            board.MovePiece(piece, dest);
            GameSignals.RaisePieceMoved(piece, from, dest, cost);
            return true;
        }

        // blocked by ally (shouldn't be in legal set)
        return false;
    }

    static void FireTileEvents(Piece piece, BoardRuntime board, Vector2Int from, Vector2Int to)
    {
        var fromTile = board.builder.tiles[from.x, from.y];
        var toTile = board.builder.tiles[to.x, to.y];

        if (fromTile?.data?.tileRules != null)
            foreach (var tr in fromTile.data.tileRules) tr.OnLeave(piece, board, fromTile);

        if (toTile?.data?.tileRules != null)
            foreach (var tr in toTile.data.tileRules) tr.OnEnter(piece, board, toTile);
    }
    
    static Vector2Int? ComputeApproachCell(BoardRuntime board, Vector2Int from, Vector2Int target)
    {
        int dx = target.x - from.x;
        int dy = target.y - from.y;

        // Only handle rook/bishop/queen lines (orthogonal/diagonal)
        bool orth = (dx == 0) || (dy == 0);
        bool diag = Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!orth && !diag) return null;

        // Unit step toward target
        int stepX = Mathf.Clamp(dx, -1, 1);
        int stepY = Mathf.Clamp(dy, -1, 1);

        // Walk from 'from + step' up to the cell before 'target'
        var cur = new Vector2Int(from.x + stepX, from.y + stepY);
        Vector2Int? lastFree = null;

        while (cur != target)
        {
            if (!board.InBounds(cur)) return null;          // out of board? bail
            if (board.GetPiece(cur) != null) return null;   // blocked before target → illegal in practice
            lastFree = cur;
            cur = new Vector2Int(cur.x + stepX, cur.y + stepY);
        }

        // lastFree is now the cell adjacent to target (on the line)
        return lastFree;
    }
    
}

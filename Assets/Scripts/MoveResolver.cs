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

        if (target != null && target.Team != piece.Team)
        {
            // Attack flow (no counterattack)
            var outcome = CombatResolver.ResolveAttack(piece, target);

            if (outcome.targetDied)
            {
                // remove defender
                Object.Destroy(target.gameObject);
                board.pieces[dest.x, dest.y] = null;

                // tile events for moving
                FireTileEvents(piece, board, from, dest);

                // advance into captured tile
                board.MovePiece(piece, dest);

                GameSignals.RaisePieceMoved(piece, from, dest, cost);
                GameSignals.RaiseAttackResolved(piece, dest, true);
            }
            else
            {
                // target lives: attacker stays in place (no tile enter/leave)
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
        var toTile   = board.builder.tiles[to.x, to.y];

        if (fromTile?.data?.tileRules != null)
            foreach (var tr in fromTile.data.tileRules) tr.OnLeave(piece, board, fromTile);

        if (toTile?.data?.tileRules != null)
            foreach (var tr in toTile.data.tileRules) tr.OnEnter(piece, board, toTile);
    }
}

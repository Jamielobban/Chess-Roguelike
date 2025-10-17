using UnityEngine;

public static class MoveResolver
{
    /// <summary>
    /// Tries to execute a move: checks energy, spends it, fires tile events, captures, and moves.
    /// Returns true if the move was performed.
    /// </summary>
    public static bool TryExecuteMove(Piece piece, Vector2Int dest, int cost, BoardRuntime board, bool spendEnergy = true)
    {
        if (piece == null || board == null) return false;

        // Notify start (for intent SFX, previews, etc.)
        GameSignals.RaiseMoveStarted(piece, dest);

        // Energy check & spend
        if (spendEnergy)
        {
            var tm = TurnManager.Instance;
            if (tm == null)
            {
                Debug.LogError("MoveResolver: TurnManager.Instance is null.");
                return false;
            }
            if (!tm.TrySpend(cost))
            {
                GameSignals.RaiseMoveFailedInsufficientEnergy(piece, dest, cost);
                return false;
            }
        }

        var from = piece.GridPos;
        var targetPiece = board.GetPiece(dest) as Piece;

        // Tile leave/enter hooks (before we actually move)
        FireTileEvents(piece, board, from, dest);

        // Capture (emit before destruction so listeners can read victim data)
        if (targetPiece != null && targetPiece.Team != piece.Team)
        {
            GameSignals.RaisePieceCaptured(piece, targetPiece, dest);
            Object.Destroy(targetPiece.gameObject);
            board.pieces[dest.x, dest.y] = null;
        }

        // Move on the board (updates grid + transform)
        board.MovePiece(piece, dest);

        // Final event after state change
        GameSignals.RaisePieceMoved(piece, from, dest, cost);

        return true;
    }

    static void FireTileEvents(Piece piece, BoardRuntime board, Vector2Int from, Vector2Int to)
    {
        var fromTile = board.builder.tiles[from.x, from.y];
        var toTile   = board.builder.tiles[to.x, to.y];

        if (fromTile?.data?.tileRules != null)
            foreach (var tr in fromTile.data.tileRules)
                tr.OnLeave(piece, board, fromTile);

        if (toTile?.data?.tileRules != null)
            foreach (var tr in toTile.data.tileRules)
                tr.OnEnter(piece, board, toTile);
    }
}

using UnityEngine;

public static class MoveResolver
{
    /// <summary>
    /// Executes a move already validated by MoveGenerator.
    /// </summary>
    public static void ExecuteMove(Piece piece, Vector2Int dest, BoardRuntime board)
    {
        if (piece == null || board == null) return;

        var from = piece.GridPos;
        var targetPiece = board.GetPiece(dest);

        // capture (optional: play animation, add score, etc.)
        if (targetPiece != null && targetPiece.Team != piece.Team)
        {
            Object.Destroy(targetPiece.gameObject);
            board.pieces[dest.x, dest.y] = null;
        }

        // fire tile exit/enter hooks
        FireTileEvents(piece, board, from, dest);

        // move
        board.MovePiece(piece, dest);
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

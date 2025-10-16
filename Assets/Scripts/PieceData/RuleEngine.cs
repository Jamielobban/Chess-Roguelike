using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RuleEngine
{
    public static List<Vector2Int> GetLegalMoves(Piece piece, BoardRuntime board)
    {
        if (piece?.data == null) return new();

        // 1) base union of all piece move rules
        IEnumerable<Vector2Int> moves = Enumerable.Empty<Vector2Int>();
        if (piece.data.moveRules != null)
            foreach (var r in piece.data.moveRules)
                moves = moves.Concat(r.GetMoves(piece, board));

        // 2) piece-level modifiers (base + runtime)
        IEnumerable<Modifier> mods = Enumerable.Empty<Modifier>();
        if (piece.data.baseModifiers != null) mods = mods.Concat(piece.data.baseModifiers);
        if (piece.runtimeMods != null)        mods = mods.Concat(piece.runtimeMods);

        foreach (var m in mods)
            moves = m.PostMovesFilter(piece, board, moves);

        // 3) origin tile rules
        var originTile = board.builder.tiles[piece.GridPos.x, piece.GridPos.y];
        if (originTile?.data?.tileRules != null)
            foreach (var tr in originTile.data.tileRules)
                moves = tr.AffectMovesOnOrigin(piece, board, moves);

        // 4) destination checks (e.g., forbidden tile types)
        var final = new List<Vector2Int>();
        foreach (var dest in moves)
        {
            if (!board.InBounds(dest)) continue;

            bool ok = true;
            var destTile = board.builder.tiles[dest.x, dest.y];
            if (destTile?.data?.tileRules != null)
                foreach (var tr in destTile.data.tileRules)
                    ok &= tr.AllowEnter(piece, board, dest);

            if (ok) final.Add(dest);
        }

        // Dedup
        return final.Distinct().ToList();
    }

    // Call these around MovePiece
    public static void FireLeaveEnter(Piece piece, BoardRuntime board, Vector2Int from, Vector2Int to)
    {
        var fromTile = board.builder.tiles[from.x, from.y];
        var toTile   = board.builder.tiles[to.x, to.y];

        if (fromTile?.data?.tileRules != null)
            foreach (var tr in fromTile.data.tileRules) tr.OnLeave(piece, board, fromTile);
        if (toTile?.data?.tileRules != null)
            foreach (var tr in toTile.data.tileRules) tr.OnEnter(piece, board, toTile);
    }
}

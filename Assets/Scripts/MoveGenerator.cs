using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MoveOption
{
    public Vector2Int dest;
    public int cost;
    public bool isCapture;
}

public static class MoveGenerator
{
    public static List<MoveOption> GetMoves(Piece piece, BoardRuntime board)
    {
        if (piece?.data == null) return new();

        IEnumerable<Vector2Int> moves = Enumerable.Empty<Vector2Int>();

        // 1. combine all MoveRules
        if (piece.data.moveRules != null)
            foreach (var r in piece.data.moveRules)
                moves = moves.Concat(r.GetMoves(piece, board));

        // 2. apply modifiers (base + runtime)
        IEnumerable<Modifier> mods = Enumerable.Empty<Modifier>();
        if (piece.data.baseModifiers != null) mods = mods.Concat(piece.data.baseModifiers);
        if (piece.runtimeMods != null)        mods = mods.Concat(piece.runtimeMods);

        foreach (var m in mods)
            moves = m.PostMovesFilter(piece, board, moves);

        // 3. origin tile effects
        var originTile = board.builder.tiles[piece.GridPos.x, piece.GridPos.y];
        if (originTile?.data?.tileRules != null)
            foreach (var tr in originTile.data.tileRules)
                moves = tr.AffectMovesOnOrigin(piece, board, moves);

        // 4. build options
        var result = new List<MoveOption>();
        foreach (var dest in moves.Distinct())
        {
            if (!board.InBounds(dest)) continue;

            var occ = board.GetPiece(dest);
            if (occ != null && occ.Team == piece.Team) continue;

            int baseCost = Mathf.Max(Mathf.Abs(dest.x - piece.GridPos.x), Mathf.Abs(dest.y - piece.GridPos.y));
            if (IsKnightLike(piece)) baseCost = 2;

            // tile AllowEnter filters
            var destTile = board.builder.tiles[dest.x, dest.y];
            if (destTile?.data?.tileRules != null)
            {
                bool ok = true;
                foreach (var tr in destTile.data.tileRules)
                    ok &= tr.AllowEnter(piece, board, dest);
                if (!ok) continue;
            }

            // modifiers can tweak cost
            int cost = baseCost;
            foreach (var m in mods)
                cost = m.ModifyMoveCost(piece, piece.GridPos, dest, cost);

            result.Add(new MoveOption
            {
                dest = dest,
                cost = Mathf.Max(1, cost),
                isCapture = (occ != null && occ.Team != piece.Team)
            });
        }

        return result;
    }

    static bool IsKnightLike(Piece p)
        => p.data.moveRules.Any(r => r is KnightJumps);
}

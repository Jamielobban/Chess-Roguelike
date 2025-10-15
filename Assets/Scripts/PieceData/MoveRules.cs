using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/MoveRules/Rook Rays")]
public class RookRays : MoveRule
{
    static readonly Vector2Int[] D = { new(1,0), new(-1,0), new(0,1), new(0,-1) };
    public override IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board)
    {
        foreach (var dir in D)
        {
            var p = piece.GridPos + dir;
            while (board.InBounds(p))
            {
                var occ = board.GetPiece(p);
                if (occ == null) { yield return p; p += dir; continue; }
                if (occ.data != null && occ != piece && occ != null) yield return p; // capture
                break;
            }
        }
    }
}

[CreateAssetMenu(menuName="RogueChess/MoveRules/Knight Jumps")]
public class KnightJumps : MoveRule
{
    static readonly Vector2Int[] J = {
        new(1,2),new(2,1),new(-1,2),new(2,-1),
        new(-2,1),new(1,-2),new(-1,-2),new(-2,-1)
    };
    public override IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board)
    {
        foreach (var o in J)
        {
            var p = piece.GridPos + o;
            if (!board.InBounds(p)) continue;
            var occ = board.GetPiece(p);
            if (occ == null || occ != null && occ != piece) yield return p;
        }
    }
}

[CreateAssetMenu(menuName="RogueChess/MoveRules/Pawn Forward")]
public class PawnForward : MoveRule
{
    public int dir = 1; // white=+1, black=-1 or use piece tag
    public bool captureDiagonals = true;

    public override IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board)
    {
        var f = piece.GridPos + new Vector2Int(0, dir);
        if (board.InBounds(f) && board.GetPiece(f) == null) yield return f;

        if (captureDiagonals)
        {
            foreach (var dx in new[] { -1, 1 })
            {
                var c = piece.GridPos + new Vector2Int(dx, dir);
                if (!board.InBounds(c)) continue;
                var occ = board.GetPiece(c);
                if (occ != null && occ != piece) yield return c;
            }
        }
    }
}

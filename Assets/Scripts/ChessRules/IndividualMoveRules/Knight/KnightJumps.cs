using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


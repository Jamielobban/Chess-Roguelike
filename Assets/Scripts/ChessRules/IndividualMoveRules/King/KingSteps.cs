using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/MoveRules/King Steps")]
public class KingSteps : MoveRule
{
    static readonly Vector2Int[] Dirs = {
        new(1,0), new(-1,0), new(0,1), new(0,-1),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    public override IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board)
    {
        foreach (var d in Dirs)
        {
            var p = piece.GridPos + d;
            if (!board.InBounds(p)) continue;

            var occ = board.GetPiece(p);
            if (occ == null || occ.Team != piece.Team)
                yield return p;
        }
    }
}

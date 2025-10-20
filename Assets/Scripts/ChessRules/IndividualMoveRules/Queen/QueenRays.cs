using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/MoveRules/Queen Rays")]
public class QueenRays : MoveRule
{
    static readonly Vector2Int[] Dirs = {
        new(1,0), new(-1,0), new(0,1), new(0,-1),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    public override IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board)
    {
        foreach (var dir in Dirs)
        {
            var p = piece.GridPos + dir;
            while (board.InBounds(p))
            {
                var occ = board.GetPiece(p);
                if (occ == null)
                {
                    yield return p;
                    p += dir;
                    continue;
                }

                if (occ.Team != piece.Team)
                    yield return p;

                break; // blocked
            }
        }
    }
}

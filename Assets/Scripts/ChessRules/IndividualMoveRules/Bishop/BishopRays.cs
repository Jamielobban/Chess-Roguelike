using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/MoveRules/Bishop Rays")]
public class BishopRays : MoveRule
{
    static readonly Vector2Int[] Dirs = {
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
                    yield return p; // capture

                break; // stop ray
            }
        }
    }
}

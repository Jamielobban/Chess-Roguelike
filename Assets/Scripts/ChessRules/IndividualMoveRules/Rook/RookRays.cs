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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

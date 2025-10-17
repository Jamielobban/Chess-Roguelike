using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/Modifiers/Step Plus One")]
public class StepPlusOne : Modifier
{
    static readonly Vector2Int[] N8 = {
        new(1,0),new(-1,0),new(0,1),new(0,-1),
        new(1,1),new(1,-1),new(-1,1),new(-1,-1)
    };
    public override IEnumerable<Vector2Int> PostMovesFilter(Piece piece, BoardRuntime board, IEnumerable<Vector2Int> moves)
    {
        var set = new HashSet<Vector2Int>(moves);
        foreach (var d in N8)
        {
            var p = piece.GridPos + d;
            if (board.InBounds(p) && (board.GetPiece(p) == null || board.GetPiece(p) != piece))
                set.Add(p);
        }
        return set;
    }
}


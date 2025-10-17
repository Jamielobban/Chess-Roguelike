using System.Collections.Generic;
using UnityEngine;

public abstract class MoveRule : ScriptableObject
{
    public abstract IEnumerable<Vector2Int> GetMoves(Piece piece, BoardRuntime board);
}

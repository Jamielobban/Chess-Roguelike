using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    public virtual IEnumerable<Vector2Int> PostMovesFilter(Piece piece, BoardRuntime board, IEnumerable<Vector2Int> moves)
    {
        return moves; 
    }

    public virtual int ModifyAttack(Piece piece, int atk) => atk;
    public virtual void OnEnterTile(Piece piece, Tile2D tile, BoardRuntime board) { }
    public virtual void OnLeaveTile(Piece piece, Tile2D tile, BoardRuntime board) { }
     public virtual int ModifyMoveCost(Piece piece, Vector2Int from, Vector2Int to, int baseCost) => baseCost;
}

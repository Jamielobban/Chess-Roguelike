using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    // Hook to transform the candidate move set
    public virtual IEnumerable<Vector2Int> PostMovesFilter(Piece piece, BoardRuntime board, IEnumerable<Vector2Int> moves)
    {
        return moves; // default no change
    }

    // Optional stat hooks
    public virtual int ModifyAttack(Piece piece, int atk) => atk;
    public virtual void OnEnterTile(Piece piece, Tile2D tile, BoardRuntime board) { }
    public virtual void OnLeaveTile(Piece piece, Tile2D tile, BoardRuntime board) { }
}

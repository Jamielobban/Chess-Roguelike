using System.Collections.Generic;
using UnityEngine;

public abstract class TileRule : ScriptableObject
{
    // Filter/extend moves while standing on this tile
    public virtual IEnumerable<Vector2Int> AffectMovesOnOrigin(Piece piece, BoardRuntime board, IEnumerable<Vector2Int> moves) => moves;

    // Check when moving into destination
    public virtual bool AllowEnter(Piece piece, BoardRuntime board, Vector2Int dest) => true;

    // Triggers
    public virtual void OnEnter(Piece piece, BoardRuntime board, Tile2D tile) { }
    public virtual void OnLeave(Piece piece, BoardRuntime board, Tile2D tile) { }
}

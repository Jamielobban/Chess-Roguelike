using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    // ---- Movement / generation ----
    public virtual IEnumerable<Vector2Int> PostMovesFilter(
        Piece piece, BoardRuntime board, IEnumerable<Vector2Int> moves) => moves;

    public virtual int ModifyMoveCost(Piece piece, Vector2Int from, Vector2Int to, int baseCost) => baseCost;

    // ---- Stats (runtime-computed) ----
    public virtual int ModifyAttack(Piece piece, int atk) => atk;
    public virtual int ModifyMaxHP(Piece piece, int baseMaxHP) => baseMaxHP;

    // ---- Tile lifecycle ----
    public virtual void OnEnterTile(Piece piece, Tile2D tile, BoardRuntime board) { }
    public virtual void OnLeaveTile(Piece piece, Tile2D tile, BoardRuntime board) { }

    // ---- Combat lifecycle (optional) ----
    public virtual void OnDamageTaken(Piece piece, int amount) { }
    public virtual void OnKill(Piece killer, Piece victim) { }

    // ---- Attachment lifecycle (optional) ----
    public virtual void OnAttach(Piece piece) { }
    public virtual void OnDetach(Piece piece) { }
}

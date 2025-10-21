using System;
using UnityEngine;

public static class GameSignals
{
    // ---------- Movement & Energy (yours, preserved) ----------
    public static event Action<Piece, Vector2Int> OnMoveStarted;
    public static event Action<Piece, Vector2Int, int> OnMoveFailedInsufficientEnergy;
    public static event Action<Piece, Piece, Vector2Int> OnPieceCaptured; // if you keep capture separate
    public static event Action<Piece, Vector2Int, Vector2Int, int> OnPieceMoved;

    public static event Action<int,int> OnPreviewEnergyChanged; // (preview, max)

    public static void RaiseMoveStarted(Piece p, Vector2Int to) =>
        OnMoveStarted?.Invoke(p, to);

    public static void RaiseMoveFailedInsufficientEnergy(Piece p, Vector2Int to, int cost) =>
        OnMoveFailedInsufficientEnergy?.Invoke(p, to, cost);

    public static void RaisePieceCaptured(Piece captor, Piece victim, Vector2Int at) =>
        OnPieceCaptured?.Invoke(captor, victim, at);

    public static void RaisePieceMoved(Piece p, Vector2Int from, Vector2Int to, int cost) =>
        OnPieceMoved?.Invoke(p, from, to, cost);

    public static void RaisePreviewEnergyChanged(int preview, int max) =>
        OnPreviewEnergyChanged?.Invoke(preview, max);


    // ---------- Combat (new) ----------
    // Fired when an attack intent begins (before any damage)
    public static event Action<Piece, Vector2Int> OnAttackStarted;              // attacker, targetCell
    // Fired after damage is applied
    public static event Action<Piece, Piece, int> OnAttackHit;                  // attacker, defender, damage
    // Fired when defender dies (before it’s destroyed, if you want to read its data)
    public static event Action<Piece, Piece> OnUnitKilled;                      // killer, victim
    // Fired when a combat/move action finishes (whether kill or not)
    public static event Action<Piece, Vector2Int, bool> OnAttackResolved;       // attacker, targetCell, targetDied

    public static void RaiseAttackStarted(Piece attacker, Vector2Int targetCell) =>
        OnAttackStarted?.Invoke(attacker, targetCell);

    public static void RaiseAttackHit(Piece attacker, Piece defender, int damage) =>
        OnAttackHit?.Invoke(attacker, defender, damage);

    public static void RaiseUnitKilled(Piece killer, Piece victim) =>
        OnUnitKilled?.Invoke(killer, victim);

    public static void RaiseAttackResolved(Piece attacker, Vector2Int targetCell, bool targetDied) =>
        OnAttackResolved?.Invoke(attacker, targetCell, targetDied);


    // ---------- HP / XP / Level (new) ----------
    public static event Action<Piece, int, int, int> OnHPChanged;               // piece, oldHP, newHP, maxHP
    public static event Action<Piece, int, int> OnXPChanged;                    // piece, oldXP, newXP
    public static event Action<Piece, int> OnLevelUp;                           // piece, newLevel

    public static void RaiseHPChanged(Piece p, int oldHP, int newHP, int maxHP) =>
        OnHPChanged?.Invoke(p, oldHP, newHP, maxHP);

    public static void RaiseXPChanged(Piece p, int oldXP, int newXP) =>
        OnXPChanged?.Invoke(p, oldXP, newXP);

    public static void RaiseLevelUp(Piece p, int newLevel) =>
        OnLevelUp?.Invoke(p, newLevel);
}

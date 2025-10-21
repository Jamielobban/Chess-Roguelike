using UnityEngine;

public class GameSignalLogger : MonoBehaviour
{
    void OnEnable()
    {
        // Movement / Energy
        GameSignals.OnMoveStarted                     += HandleMoveStarted;
        GameSignals.OnMoveFailedInsufficientEnergy    += HandleMoveFailedEnergy;
        GameSignals.OnPieceCaptured                   += HandlePieceCaptured;
        GameSignals.OnPieceMoved                      += HandlePieceMoved;
        GameSignals.OnPreviewEnergyChanged            += HandlePreviewEnergy;

        // Combat
        GameSignals.OnAttackStarted                   += HandleAttackStarted;
        GameSignals.OnAttackHit                       += HandleAttackHit;
        GameSignals.OnUnitKilled                      += HandleUnitKilled;
        GameSignals.OnAttackResolved                  += HandleAttackResolved;

        // HP / XP / Level
        GameSignals.OnHPChanged                       += HandleHPChanged;
        GameSignals.OnXPChanged                       += HandleXPChanged;
        GameSignals.OnLevelUp                         += HandleLevelUp;
    }

    void OnDisable()
    {
        // IMPORTANT: always mirror the += / -= pairs
        GameSignals.OnMoveStarted                     -= HandleMoveStarted;
        GameSignals.OnMoveFailedInsufficientEnergy    -= HandleMoveFailedEnergy;
        GameSignals.OnPieceCaptured                   -= HandlePieceCaptured;
        GameSignals.OnPieceMoved                      -= HandlePieceMoved;
        GameSignals.OnPreviewEnergyChanged            -= HandlePreviewEnergy;

        GameSignals.OnAttackStarted                   -= HandleAttackStarted;
        GameSignals.OnAttackHit                       -= HandleAttackHit;
        GameSignals.OnUnitKilled                      -= HandleUnitKilled;
        GameSignals.OnAttackResolved                  -= HandleAttackResolved;

        GameSignals.OnHPChanged                       -= HandleHPChanged;
        GameSignals.OnXPChanged                       -= HandleXPChanged;
        GameSignals.OnLevelUp                         -= HandleLevelUp;
    }

    // -------- Handlers --------

    // Movement / Energy
    void HandleMoveStarted(Piece p, Vector2Int to) =>
        Debug.Log($"[SIG] MoveStarted → {p?.name} → {to}");

    void HandleMoveFailedEnergy(Piece p, Vector2Int to, int cost) =>
        Debug.Log($"[SIG] MoveFailedEnergy → {p?.name} to {to}, cost {cost}");

    void HandlePieceCaptured(Piece captor, Piece victim, Vector2Int at) =>
        Debug.Log($"[SIG] PieceCaptured → {captor?.name} captured {victim?.name} at {at}");

    void HandlePieceMoved(Piece p, Vector2Int from, Vector2Int to, int cost) =>
        Debug.Log($"[SIG] PieceMoved → {p?.name} {from} → {to}, cost {cost}");

    void HandlePreviewEnergy(int preview, int max) =>
        Debug.Log($"[SIG] PreviewEnergyChanged → {preview}/{max}");

    // Combat
    void HandleAttackStarted(Piece attacker, Vector2Int targetCell) =>
        Debug.Log($"[SIG] AttackStarted → {attacker?.name} targeting {targetCell}");

    void HandleAttackHit(Piece a, Piece d, int dmg) =>
        Debug.Log($"[SIG] AttackHit → {a?.name} dealt {dmg} to {d?.name}");

    void HandleUnitKilled(Piece killer, Piece victim) =>
        Debug.Log($"[SIG] UnitKilled → {killer?.name} killed {victim?.name}");

    void HandleAttackResolved(Piece a, Vector2Int to, bool targetDied) =>
        Debug.Log($"[SIG] AttackResolved → {a?.name} to {to}, targetDied={targetDied}");

    // HP / XP / Level
    void HandleHPChanged(Piece p, int oldHP, int newHP, int maxHP) =>
        Debug.Log($"[SIG] HPChanged → {p?.name} {oldHP}->{newHP}/{maxHP}");

    void HandleXPChanged(Piece p, int oldXP, int newXP) =>
        Debug.Log($"[SIG] XPChanged → {p?.name} {oldXP}->{newXP}");

    void HandleLevelUp(Piece p, int lvl) =>
        Debug.Log($"[SIG] LevelUp → {p?.name} now Lv{lvl}");
}

using System;
using UnityEngine;

public static class GameSignals
{
    /// <summary>Emitted right before we attempt a move (affordability not yet checked).</summary>
    public static event Action<Piece, Vector2Int> OnMoveStarted;

    /// <summary>Emitted when move fails due to insufficient energy.</summary>
    public static event Action<Piece, Vector2Int, int> OnMoveFailedInsufficientEnergy;

    /// <summary>Emitted when a capture is about to happen (victim still exists).</summary>
    public static event Action<Piece, Piece, Vector2Int> OnPieceCaptured;

    /// <summary>Emitted after a successful move and board state updated.</summary>
    public static event Action<Piece, Vector2Int, Vector2Int, int> OnPieceMoved;

    public static event Action<int,int> OnPreviewEnergyChanged; // (preview, max)

    // Helper “raise” methods (optional)
    public static void RaiseMoveStarted(Piece p, Vector2Int to) =>
        OnMoveStarted?.Invoke(p, to);

    public static void RaiseMoveFailedInsufficientEnergy(Piece p, Vector2Int to, int cost) =>
        OnMoveFailedInsufficientEnergy?.Invoke(p, to, cost);

    public static void RaisePieceCaptured(Piece captor, Piece victim, Vector2Int at) =>
        OnPieceCaptured?.Invoke(captor, victim, at);

    public static void RaisePieceMoved(Piece p, Vector2Int from, Vector2Int to, int cost) =>
        OnPieceMoved?.Invoke(p, from, to, cost);
    public static void RaisePreviewEnergyChanged(int preview, int max)
        => OnPreviewEnergyChanged?.Invoke(preview, max);
}

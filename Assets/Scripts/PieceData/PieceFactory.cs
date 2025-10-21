// PieceFactory.cs
using UnityEngine;

public class PieceFactory : MonoBehaviour
{
    public static PieceFactory Instance { get; private set; }
    public Piece piecePrefab;
    public BoardRuntime board;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!board) board = BoardRuntime.Instance;
    }

    public Piece Spawn(PieceData data, Vector2Int cell)
    {
        if (!board || !piecePrefab || !data) return null;
        var p = Instantiate(piecePrefab);
        p.Init(data);                        
        board.PlacePiece(p, cell);
        return p;
    }
}

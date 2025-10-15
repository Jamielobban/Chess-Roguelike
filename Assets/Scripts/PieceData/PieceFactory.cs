using UnityEngine;

public class PieceFactory : MonoBehaviour
{
    public static PieceFactory Instance { get; private set; }
    public Piece piecePrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Piece Spawn(PieceData data, Vector2Int cell)
    {
        var board = BoardRuntime.Instance;
        if (board == null)
        {
            Debug.LogError("No BoardRuntime found in scene.");
            return null;
        }

        var p = Instantiate(piecePrefab);
        p.Init(data);
        board.PlacePiece(p, cell);
        return p;
    }
}

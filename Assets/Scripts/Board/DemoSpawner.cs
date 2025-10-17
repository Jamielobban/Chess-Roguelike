using UnityEngine;

public class DemoSpawner : MonoBehaviour
{
    public BoardBuilder2D board;
    public PieceFactory factory;
    public PieceData whitePawn;
    public PieceData blackPawn;
    public PieceData knightPiece;
    public PieceData rookPiece;

    void Start()
    {
        //board.Build();
        factory.Spawn(knightPiece, new Vector2Int(2, 2));
        factory.Spawn(rookPiece, new Vector2Int(3, 7));
    }
}

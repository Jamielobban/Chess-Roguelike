using UnityEditor.Search;
using UnityEngine;

public class DemoSpawner : MonoBehaviour
{
    public BoardBuilder2D board;
    public PieceFactory factory;
    [Header("White")]
    public PieceData whitePawn;
    public PieceData whiteKnight;
    public PieceData whiteRook;
    public PieceData whiteBishop;
    public PieceData whiteQueen;
    public PieceData whiteKing;
    [Header("Black")]
    public PieceData blackPawn;
    public PieceData blackKnight;
    public PieceData blackRook;
    public PieceData blackBishop;
    public PieceData blackQueen;
    public PieceData blackKing;

    void Start()
    {
        //board.Build();
        factory.Spawn(blackRook, new Vector2Int(2, 2));
        factory.Spawn(whiteRook, new Vector2Int(3, 7));
        factory.Spawn(whiteBishop, new Vector2Int(2, 7));
        //factory.Spawn(blackQueen, new Vector2Int(1, 7));        
    }
}

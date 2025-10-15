using UnityEngine;

public class DemoSpawner : MonoBehaviour
{
    public BoardBuilder2D board;
    public PieceFactory factory;
    public PieceData whitePawn;
    public PieceData blackPawn;

    void Start()
    {
        //board.Build();
        factory.Spawn(whitePawn, new Vector2Int(2, 2));
        factory.Spawn(blackPawn, new Vector2Int(3, 7));
    }
}

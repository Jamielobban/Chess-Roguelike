using UnityEngine;

public class BoardRuntime : MonoBehaviour
{
    public static BoardRuntime Instance { get; private set; }

    [Header("Refs")]
    public BoardBuilder2D builder;   // assign from scene or auto-find

    public Piece[,] pieces;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
        if (builder != null)
        {
            //builder.Build(); // ensure tiles exist
            pieces = new Piece[builder.cols, builder.rows];
        }
        else
        {
            Debug.LogError("BoardRuntime: No BoardBuilder2D found in scene.");
        }
          Debug.Log($"BoardRuntime Awake. cols={builder?.cols}, rows={builder?.rows}");
    }

    public bool InBounds(Vector2Int c)
    {
        return builder && c.x >= 0 && c.y >= 0 && c.x < builder.cols && c.y < builder.rows;
    }

    public Piece GetPiece(Vector2Int c)
    {
        if (pieces == null || !InBounds(c)) return null;
        return pieces[c.x, c.y];
    }

    public void PlacePiece(Piece p, Vector2Int c)
    {
        if (!InBounds(c) || p == null) return;
        if (pieces[c.x, c.y] != null)
            Debug.LogWarning($"Cell {c} already occupied.");

        pieces[c.x, c.y] = p;
        p.GridPos = c;
        p.transform.position = builder.GridToWorld(c);
        Debug.Log($"PlacePiece {p.name} -> {c}");
    }

    public void MovePiece(Piece p, Vector2Int to)
    {
        if (!InBounds(to) || p == null) return;

        if (pieces[p.GridPos.x, p.GridPos.y] == p)
            pieces[p.GridPos.x, p.GridPos.y] = null;

        var target = pieces[to.x, to.y];
        if (target != null && target != p)
            Destroy(target.gameObject);

        pieces[to.x, to.y] = p;
        p.GridPos = to;
        p.transform.position = builder.GridToWorld(to);
    }
}

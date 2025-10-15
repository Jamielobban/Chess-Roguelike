using System.Collections.Generic;
using UnityEngine;

public class Picker2D : MonoBehaviour
{
    public Camera cam;
    public InputManager input;              // fires OnPrimaryClick
    public BoardRuntime runtime;            // optional: assign, else auto-find
    public BoardBuilder2D builder;          // optional: assign, else auto-find

    public Color selectColor = new(1f, 0.95f, 0.2f, 1f);
    public Color moveColor   = new(0.2f, 0.9f, 0.3f, 1f);

    private Piece _selected;
    private readonly List<Tile2D> _lit = new();

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();

        if (runtime == null)
            Debug.LogError("Picker2D: No BoardRuntime found. Add BoardRuntime to scene.");
        if (builder == null)
            Debug.LogError("Picker2D: No BoardBuilder2D found. Add/assign a builder.");
    }

    void OnEnable()
    {
        if (input != null) input.OnPrimaryClick += HandlePrimaryClick;
    }

    void OnDisable()
    {
        if (input != null) input.OnPrimaryClick -= HandlePrimaryClick;
    }

    void HandlePrimaryClick(Vector2 screenPos)
    {
        //Debug.Log("hello1");
        if (cam == null || builder == null) return;
       // Debug.Log("hello3");

        // Convert screen → world
        var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

        // First try a raycast
        var hit = Physics2D.Raycast(world, Vector2.zero);

        Collider2D col = hit.collider;
        if (col == null)
        {
            // fallback: maybe collider is trigger-only
            col = Physics2D.OverlapPoint(world);
            if (col == null) return; // nothing hit
        }

        var tile = col.GetComponent<Tile2D>();
        Debug.Log(tile.gameObject.name);
        if (!tile) return;

        OnTileClicked(tile);
    }


    void OnTileClicked(Tile2D t)
    {
        if (runtime == null || builder == null) return;

        Debug.Log($"Clicked coord {t.coord}");
        var pieceAt = runtime.GetPiece(t.coord);
        bool hasPiece = pieceAt != null;
        Debug.Log(pieceAt ? $"Found piece {pieceAt.name} at {t.coord}" : "No piece in runtime grid at coord");

        Debug.Log(pieceAt);
        if (_selected == null)
        {
            if (hasPiece) Select(pieceAt);
            return; // empty tile with no selection -> noop
        }

        if (hasPiece)
        {
            Select(pieceAt); // reselect another piece
            return;
        }

        // empty tile and we have a selection -> move
        runtime.MovePiece(_selected, t.coord);
        ClearSelection();
    }

    void Select(Piece p)
    {
        if (p == null || builder == null || runtime == null) return;

        ClearSelection();
        _selected = p;

        // highlight selected tile
        var selTile = builder.tiles[p.GridPos.x, p.GridPos.y];
        selTile.SetHighlight(selectColor);
        _lit.Add(selTile);
    }

    void ClearSelection()
    {
        foreach (var t in _lit) t.ClearHighlight();
        _lit.Clear();
        _selected = null;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Picker2D : MonoBehaviour
{
    public Camera cam;
    public InputManager input;
    public BoardRuntime runtime;     // auto-grabbed if left empty
    public BoardBuilder2D builder;   // auto-grabbed if left empty

    public Color selectColor = new(1f, 0.95f, 0.2f, 1f);
    public Color moveColor   = new(0.2f, 0.9f, 0.3f, 1f);

    private Piece _selected;
    private readonly List<Tile2D> _lit = new();
    private readonly HashSet<Vector2Int> _legal = new();

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();

        if (!runtime) Debug.LogError("Picker2D: No BoardRuntime found.");
        if (!builder) Debug.LogError("Picker2D: No BoardBuilder2D found.");
    }

    void OnEnable()  { if (input) input.OnPrimaryClick += HandlePrimaryClick; }
    void OnDisable() { if (input) input.OnPrimaryClick -= HandlePrimaryClick; }

    void HandlePrimaryClick(Vector2 screenPos)
    {
        if (!cam || !builder) return;

        float zDist = Mathf.Abs(builder.transform.position.z - cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
        Collider2D col = Physics2D.OverlapPoint(world);
        if (!col) return;

        var tile = col.GetComponent<Tile2D>();
        if (!tile) return;

        OnTileClicked(tile);
    }

    void OnTileClicked(Tile2D t)
    {
        if (!runtime || !builder) return;

        var pieceAt = runtime.GetPiece(t.coord);
        bool hasPiece = pieceAt != null;

        // Nothing selected yet → select piece if any
        if (_selected == null)
        {
            if (hasPiece) Select(pieceAt);
            return;
        }

        // Clicked another piece → reselect
        if (hasPiece)
        {
            Select(pieceAt);
            return;
        }

        // Empty tile → only move if legal
        if (_legal.Contains(t.coord))
        {
            RuleEngine.FireLeaveEnter(_selected, runtime, _selected.GridPos, t.coord);
            runtime.MovePiece(_selected, t.coord);
            ClearSelection();
        }
        else
        {
            // optional: deselect if clicked outside
            // ClearSelection();
        }
    }

    void Select(Piece p)
    {
        if (!p || !builder || !runtime) return;

        ClearSelection();
        _selected = p;

        // highlight selected tile
        var selTile = builder.tiles[p.GridPos.x, p.GridPos.y];
        if (selTile)
        {
            selTile.SetHighlight(selectColor);
            _lit.Add(selTile);
        }

        // ======= LEGAL MOVE COMPUTATION =======
        _legal.Clear();
        var legalMoves = RuleEngine.GetLegalMoves(p, runtime);  // ← this uses all your MoveRules/Modifiers system
        foreach (var c in legalMoves)
        {
            if (!runtime.InBounds(c)) continue;
            _legal.Add(c);

            var tt = builder.tiles[c.x, c.y];
            if (tt)
            {
                tt.SetHighlight(moveColor);
                _lit.Add(tt);
            }
        }
        // ======================================
    }

    void ClearSelection()
    {
        foreach (var t in _lit) if (t) t.ClearHighlight();
        _lit.Clear();
        _legal.Clear();
        _selected = null;
    }
}

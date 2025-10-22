using System.Collections.Generic;
using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    [Header("Refs")]
    public BoardRuntime runtime;        // <- for InBounds
    public BoardBuilder2D builder;      // <- for tiles[]

    [Header("Colors")]
    public Color selectedColor = new(1f, 0.95f, 0.2f, 1f); // origin
    public Color legalColor    = new(0.2f, 0.9f, 0.3f, 1f); // legal dests
    public Color hoverColor    = new(1f, 0.6f, 0.1f, 1f);   // hovered legal dest
    public Color blockedColor  = new(0.6f, 0.2f, 0.2f, 1f); // unaffordable
    public Color pathColor     = new(0.6f, 0.8f, 1f, 1f);   // trail tiles

    // bookkeeping
    private readonly Dictionary<Vector2Int, (bool isCapture, bool affordable)> _legal = new();
    private Vector2Int? _selected;
    private Vector2Int? _hovering;
    private readonly List<Vector2Int> _path = new();

    void Awake()
    {
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();
    }

    public void ClearAll()
    {
        if (builder?.tiles != null)
        {
            foreach (var kv in _legal)
                builder.tiles[kv.Key.x, kv.Key.y]?.ClearHighlight();

            if (_selected != null)
                builder.tiles[_selected.Value.x, _selected.Value.y]?.ClearHighlight();

            if (_hovering != null)
                builder.tiles[_hovering.Value.x, _hovering.Value.y]?.ClearHighlight();

            foreach (var c in _path)
                builder.tiles[c.x, c.y]?.ClearHighlight();
        }
        _selected = null;
        _hovering = null;
        _legal.Clear();
        _path.Clear();
    }

    public void HighlightSelected(Vector2Int coord)
    {
        _selected = coord;
        builder.tiles[coord.x, coord.y].SetHighlight(selectedColor);
    }

    public void HighlightOption(Vector2Int coord, bool isCapture, bool affordable)
    {
        _legal[coord] = (isCapture, affordable);
        builder.tiles[coord.x, coord.y].SetHighlight(affordable ? legalColor : blockedColor);
    }

    /// Hover: recolor hovered legal + draw a path from selected → hovered
    public void SetHover(Vector2Int? coord)
    {
        // restore previous hovered color
        if (_hovering != null && _legal.TryGetValue(_hovering.Value, out var prevMeta))
        {
            builder.tiles[_hovering.Value.x, _hovering.Value.y]
                   .SetHighlight(prevMeta.affordable ? legalColor : blockedColor);
        }
        _hovering = null;

        // clear old path
        foreach (var c in _path)
            builder.tiles[c.x, c.y].ClearHighlight();
        _path.Clear();

        if (coord == null || _selected == null) return;

        if (_legal.TryGetValue(coord.Value, out var meta))
        {
            // set hovered color
            _hovering = coord;
            builder.tiles[coord.Value.x, coord.Value.y].SetHighlight(hoverColor);

            // draw straight/diagonal trail between selected and hovered
            foreach (var c in ComputePathStraightOrDiag(_selected.Value, coord.Value))
            {
                if (!runtime.InBounds(c)) break;
                if (c == _selected.Value || c == coord.Value) continue;

                builder.tiles[c.x, c.y].SetHighlight(pathColor);
                _path.Add(c);
            }
        }
    }

    // cells strictly between 'from' and 'to' if colinear/diagonal
    IEnumerable<Vector2Int> ComputePathStraightOrDiag(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool orth = (dx == 0) || (dy == 0);
        bool diag = Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!orth && !diag) yield break;

        int sx = Mathf.Clamp(dx, -1, 1);
        int sy = Mathf.Clamp(dy, -1, 1);

        var cur = new Vector2Int(from.x + sx, from.y + sy);
        while (cur != to)
        {
            yield return cur;
            cur = new Vector2Int(cur.x + sx, cur.y + sy);
        }
    }
}

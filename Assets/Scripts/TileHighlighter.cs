using System.Collections.Generic;
using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    [Header("Refs")]
    public BoardRuntime runtime;        // InBounds()
    public BoardBuilder2D builder;      // tiles[,]

    [Header("Colors")]
    public Color selectedColor = new(1f, 0.95f, 0.2f, 1f);   // origin
    public Color legalColor    = new(0.20f, 0.90f, 0.30f, 1f); // legal
    public Color hoverColor    = new(1.00f, 0.60f, 0.10f, 1f); // hovered legal
    public Color blockedColor  = new(0.60f, 0.20f, 0.20f, 1f); // unaffordable
    public Color pathColor     = new(0.60f, 0.80f, 1.00f, 1f); // trail

    // state
    private readonly Dictionary<Vector2Int, (bool isCapture, bool affordable)> _legal = new();
    private Vector2Int? _selected;
    private Vector2Int? _hovering;
    private readonly List<Vector2Int> _path = new();

    void Awake()
    {
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();
    }

    // -------- external API --------

    public void ClearAll()
    {
        if (builder?.tiles != null)
        {
            // collect everything we painted
            var toClear = new HashSet<Vector2Int>();
            foreach (var kv in _legal) toClear.Add(kv.Key);
            foreach (var c in _path)   toClear.Add(c);
            if (_selected != null)     toClear.Add(_selected.Value);
            if (_hovering != null)     toClear.Add(_hovering.Value);

            // reset state FIRST
            _legal.Clear();
            _path.Clear();
            _hovering = null;
            _selected = null;

            // then clear tiles
            foreach (var c in toClear)
                SafeClear(c);
        }
        else
        {
            _legal.Clear();
            _path.Clear();
            _hovering = null;
            _selected = null;
        }
    }

    public void HighlightSelected(Vector2Int coord)
    {
        _selected = coord;
        SafeSet(coord, selectedColor);
    }

    public void HighlightOption(Vector2Int coord, bool isCapture, bool affordable)
    {
        _legal[coord] = (isCapture, affordable);
        SafeSet(coord, affordable ? legalColor : blockedColor);
    }

    /// Set current hover; recolors hovered legal & draws path from selected.
    public void SetHover(Vector2Int? coord)
    {
        // --- restore previous hovered tile to its base (legal/blocked/selected/default) ---
        if (_hovering != null)
        {
            var prev = _hovering.Value;
            _hovering = null;                 // IMPORTANT: clear state first
            RestoreBaseColor(prev);
        }

        // --- clear previous path (restore underlying colors) ---
        foreach (var c in _path) RestoreBaseColor(c);
        _path.Clear();

        // nothing hovered or no selection → done
        if (coord == null || _selected == null) return;

        var dest = coord.Value;
        if (!_legal.TryGetValue(dest, out var meta)) return; // hover on non-legal tile: no highlight/path

        // paint hovered destination
        _hovering = dest;
        SafeSet(dest, hoverColor);

        // draw straight/diagonal trail
        foreach (var c in ComputePathStraightOrDiag(_selected.Value, dest))
        {
            if (!runtime.InBounds(c)) break;
            if (c == _selected.Value || c == dest) continue;

            SafeSet(c, pathColor);
            _path.Add(c);
        }
    }

    // -------- internals --------

    void RestoreBaseColor(Vector2Int c)
    {
        if (!runtime.InBounds(c)) return;

        // selected has priority
        if (_selected != null && c == _selected.Value) { SafeSet(c, selectedColor); return; }

        // hovered legal next
        if (_hovering != null && c == _hovering.Value) { SafeSet(c, hoverColor); return; }

        // legal/blocked
        if (_legal.TryGetValue(c, out var meta))
        {
            SafeSet(c, meta.affordable ? legalColor : blockedColor);
            return;
        }

        // nothing special
        SafeClear(c);
    }

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

    // safety wrappers
    void SafeSet(Vector2Int c, Color col)
    {
        if (builder?.tiles == null) return;
        if (!runtime.InBounds(c)) return;
        builder.tiles[c.x, c.y]?.SetHighlight(col);
    }

    void SafeClear(Vector2Int c)
    {
        if (builder?.tiles == null) return;
        if (!runtime.InBounds(c)) return;
        builder.tiles[c.x, c.y]?.ClearHighlight();
    }
}

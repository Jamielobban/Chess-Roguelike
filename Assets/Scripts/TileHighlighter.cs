using System.Collections.Generic;
using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    public BoardBuilder2D builder;

    [Header("Colors")]
    public Color selectedColor = new(1f, 0.95f, 0.2f, 1f);
    public Color moveColor     = new(0.2f, 0.9f, 0.3f, 1f);
    public Color captureColor  = new(0.9f, 0.2f, 0.2f, 1f);
    public Color blockedColor  = new(0.5f, 0.5f, 0.5f, 1f);

    private readonly List<Tile2D> _lit = new();

    void Awake()
    {
        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
    }

    public void ClearAll()
    {
        foreach (var t in _lit) if (t) t.ClearHighlight();
        _lit.Clear();
    }

    public void HighlightSelected(Vector2Int coord)
    {
        var t = builder.tiles[coord.x, coord.y];
        if (t) { t.SetHighlight(selectedColor); _lit.Add(t); }
    }

    public void HighlightMoves(IEnumerable<Vector2Int> coords)
    {
        foreach (var c in coords)
        {
            var t = builder.tiles[c.x, c.y];
            if (t) { t.SetHighlight(moveColor); _lit.Add(t); }
        }
    }

    public void HighlightOption(Vector2Int coord, bool isCapture, bool affordable)
    {
        var t = builder.tiles[coord.x, coord.y];
        if (!t) return;

        var color = !affordable ? blockedColor
                  : isCapture   ? captureColor
                  : moveColor;
        t.SetHighlight(color);
        _lit.Add(t);
    }
}

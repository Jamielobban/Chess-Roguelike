using UnityEngine;

/// <summary>
/// Represents a single board cell. Handles color and stores its coordinate.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Tile2D : MonoBehaviour
{
    public TileData data;
    [SerializeField] private Vector2Int _coord; // stays serialized in scene
    public Vector2Int coord => _coord;

    [HideInInspector] public BoardBuilder2D board;

    private SpriteRenderer _sr;
    private Color _baseColor;

    /// <summary>
    /// Initialize tile when first created by the builder.
    /// </summary>
    public void Init(BoardBuilder2D parent, Vector2Int c, Color baseColor)
    {
        board = parent;
        _coord = c;
        _sr = GetComponent<SpriteRenderer>();
        _baseColor = baseColor;
        _sr.color = _baseColor;
        name = $"Tile_{c.x}_{c.y}";
    }

    void Awake()
    {
        // cache SpriteRenderer in case loaded from scene
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_baseColor == default) _baseColor = _sr.color;
    }

    /// <summary>Change tile tint (used for selection / highlights).</summary>
    public void SetHighlight(Color c)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.color = c;
    }

    /// <summary>Return to its base color.</summary>
    public void ClearHighlight()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.color = _baseColor;
    }
}

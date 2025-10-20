using System;
using UnityEngine;

public class TileInputController : MonoBehaviour
{
    public Camera cam;
    public BoardBuilder2D builder;
    public InputManager input;  

    public event Action<Vector2Int, Tile2D> OnTileClicked;
    public event Action<Vector2Int?, Tile2D> OnTileHovered; 

    Vector2Int? _lastHover;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
        if (!input) input = InputManager.Instance;
    }

    void OnEnable()
    {
        if (input != null) input.OnPrimaryClick += HandlePrimaryClick;
    }

    void OnDisable()
    {
        if (input != null) input.OnPrimaryClick -= HandlePrimaryClick;
    }

    void Update()
    {
        if (cam == null || builder == null || input == null) return;

        Vector2 screen = input.MousePosition;
        float zDist = Mathf.Abs(builder.transform.position.z - cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, zDist));
        Collider2D col = Physics2D.OverlapPoint(world);

        if (!col)
        {
            if (_lastHover != null)
            {
                _lastHover = null;
                OnTileHovered?.Invoke(null, null);
            }
            return;
        }

        var tile = col.GetComponent<Tile2D>();
        if (!tile) return;
        Debug.Log(tile.coord);

        if (_lastHover == null || _lastHover.Value != tile.coord)
        {
            _lastHover = tile.coord;
            OnTileHovered?.Invoke(tile.coord, tile);
        }
    }

    void HandlePrimaryClick(Vector2 screenPos)
    {
        if (cam == null || builder == null) return;

        float zDist = Mathf.Abs(builder.transform.position.z - cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
        Collider2D col = Physics2D.OverlapPoint(world);
        if (!col) return;

        var tile = col.GetComponent<Tile2D>();
        if (!tile) return;

        OnTileClicked?.Invoke(tile.coord, tile);
    }
}

using System;
using UnityEngine;

public class TileInputController : MonoBehaviour
{
    public Camera cam;
    public InputManager input;
    public BoardBuilder2D builder;

    public event Action<Vector2Int, Tile2D> OnTileClicked;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
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
        if (!cam || !builder) return;

        float zDist = Mathf.Abs(builder.transform.position.z - cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
        Collider2D col = Physics2D.OverlapPoint(world);
        if (!col) return;

        var tile = col.GetComponent<Tile2D>();
        if (!tile) return;

        OnTileClicked?.Invoke(tile.coord, tile);
    }
}

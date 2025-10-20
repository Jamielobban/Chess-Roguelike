using UnityEngine;

[ExecuteAlways]
public class BoardBuilder2D : MonoBehaviour
{
    [Header("Board Size")]
    public int cols = 8, rows = 8;
    [Header("Visuals")]
    public Sprite lightTile, darkTile;
    public float unitSize = 1f;
    public string sortingLayerName = "Default";
    public int sortingOrder = 0;

    [System.NonSerialized] public Tile2D[,] tiles;

    void Awake()
    {
        //Debug.Log(transform.childCount);
        // only build if nothing exists
        if (transform.childCount == 0)
            Build();
        else
            CacheExistingTiles();
    }

    /// <summary>Re-link existing child tiles into the array.</summary>
    public void CacheExistingTiles()
    {
        var found = GetComponentsInChildren<Tile2D>(true);
        if (found == null || found.Length == 0) return;

        tiles = new Tile2D[cols, rows];
        foreach (var t in found)
        {
            var c = t.coord;
            if (c.x >= 0 && c.y >= 0 && c.x < cols && c.y < rows)
                tiles[c.x, c.y] = t;
        }
    }

    [ContextMenu("Build / Rebuild Board")]
    public void Build()
    {
        Debug.Log("existing here");
        // clear existing
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }

        tiles = new Tile2D[cols, rows];

        float offsetX = (cols - 1) * 0.5f * unitSize;
        float offsetY = (rows - 1) * 0.5f * unitSize;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            bool isLight = ((x + y) % 2 == 0);
            var sprite = isLight ? lightTile : darkTile;

            var go = new GameObject($"Tile_{x}_{y}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(x * unitSize - offsetX, y * unitSize - offsetY, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            var tile = go.AddComponent<Tile2D>();
            tile.Init(this, new Vector2Int(x, y),
                isLight ? Color.white : new Color(0.9f, 0.6f, 0.4f));

            tiles[x, y] = tile;
        }
    }

    public Vector3 GridToWorld(Vector2Int c)
    {
        float offsetX = (cols - 1) * 0.5f * unitSize;
        float offsetY = (rows - 1) * 0.5f * unitSize;
        return transform.TransformPoint(new Vector3(c.x * unitSize - offsetX, c.y * unitSize - offsetY, 0));
    }

    #if UNITY_EDITOR
void OnValidate()
{
    if (!Application.isPlaying)
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this) CacheExistingTiles();
        };
}
#endif

}

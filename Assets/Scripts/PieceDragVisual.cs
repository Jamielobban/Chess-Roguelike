using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PieceDragVisual : MonoBehaviour
{
    public Camera cam;
    public BoardBuilder2D builder;
    public SpriteRenderer sr;

    [Header("Feel")]
    public float followLerp = 15f;
    public float bobAmplitude = 0.05f;
    public float bobFrequency = 6f;
    public float tiltAmount = 8f;

    Vector3 _targetWorld;
    float _t;
    bool _active;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 9999;
        gameObject.SetActive(false);
    }

    // NEW: optional startWorld param; if null, we use the piece's grid position
    public void ShowFromPiece(Piece p, Vector3? startWorld = null)
    {
        if (!p) return;
        if (!builder) builder = FindFirstObjectByType<BoardBuilder2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();

        var src = p.GetComponent<SpriteRenderer>();
        sr.sprite = src ? src.sprite : null;
        sr.color  = src ? src.color : Color.white;
        transform.localScale = p.transform.lossyScale;

        Vector3 start = startWorld ?? builder.GridToWorld(p.GridPos);
        transform.position = start;
        _targetWorld = start; // prime target so it doesn’t “pop”
        _t = 0f;

        _active = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _active = false;
        gameObject.SetActive(false);
    }

    public void SetTargetFromScreen(Vector2 screenPos)
    {
        if (!cam || !builder) return;
        // Use the board plane distance so it works in ortho/perspective
        float zDist = Mathf.Abs(builder.transform.position.z - cam.transform.position.z);
        _targetWorld = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
    }

    public void SetTint(Color c) { if (sr) sr.color = c; }

    void Update()
    {
        if (!_active) return;
        _t += Time.deltaTime;

        var pos = transform.position;
        var desired = _targetWorld;
        var to = Vector3.Lerp(pos, desired, 1f - Mathf.Exp(-followLerp * Time.deltaTime));

        to.y += Mathf.Sin(_t * bobFrequency) * bobAmplitude;

        var v = (to - pos) / Mathf.Max(Time.deltaTime, 0.0001f);
        float angle = Mathf.Clamp(v.x * 0.02f, -1f, 1f) * tiltAmount;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        transform.position = to;
    }
}

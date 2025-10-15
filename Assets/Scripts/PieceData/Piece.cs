using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Piece : MonoBehaviour
{
    public PieceData data;
    public Vector2Int GridPos;
    public int HP { get; private set; }
    public List<Modifier> runtimeMods = new(); // stack changes over time

    SpriteRenderer _sr;

    public void Init(PieceData d)
    {
        data = d;
        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = d.sprite; _sr.color = d.tint;
        HP = d.maxHP;
        name = d.displayName;
    }

    //public IEnumerable<string> Tags => data?.tags ?? System.Array.Empty<string>();
}

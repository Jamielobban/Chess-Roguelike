using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Piece : MonoBehaviour
{
    public PieceData data;
    public Vector2Int GridPos;
    public int currentHP;

    SpriteRenderer sr;

    public void Init(PieceData d)
    {
        data = d;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = data.sprite;
        sr.color = data.tint;
        currentHP = data.maxHP;
    }
}

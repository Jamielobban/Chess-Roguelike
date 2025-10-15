using UnityEngine;

[CreateAssetMenu(menuName = "ChessRogue/Piece Data")]
public class PieceData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Piece";
    public Sprite sprite;
    public Color tint = Color.white;

    [Header("Stats")]
    public int maxHP = 1;
    public int attack = 1;
    public int moveRange = 1; // placeholder for now

    [Header("Flags")]
    public bool isEnemy = false;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/Piece Data")]
public class PieceData : ScriptableObject
{
    public string displayName;
    public Sprite sprite;
    public Color tint = Color.white;
    public Team team = Team.White;

    [Header("Base stats")]
    public int maxHP = 1;
    public int attack = 1;

    [Header("Rules")]
    public List<MoveRule> moveRules;
    public List<Modifier> baseModifiers; // e.g., “LongStride”, “CanPassThroughAllies”
    public List<string> tags;            // “Undead”, “Fire”, etc.
}

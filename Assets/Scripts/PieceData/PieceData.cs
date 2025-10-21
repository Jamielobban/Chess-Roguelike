// PieceData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/Piece Data")]
public class PieceData : ScriptableObject
{
    public string displayName;
    public Team team;              // White or Black
    public Sprite sprite;          // Use the team-specific sprite here
    public Color tint = Color.white;

    [Header("Stats")]
    public int maxHP = 1;
    public int attack = 1;

    [Header("Rules")]
    public List<MoveRule> moveRules;
    public List<Modifier> baseModifiers;
    public List<string> tags;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/Enemies/Enemy Blueprint")]
public class EnemyBlueprint : ScriptableObject
{
    [Header("Base")]
    public string displayName;
    public PieceData piece;              // reuse your PieceData (sprite, rules, etc.)

    [Header("Wave Drafting")]
    public int cost = 1;                 // used for budget-based waves
    public int minCount = 1;
    public int maxCount = 1;
    [Range(0.01f, 10f)] public float weight = 1f; // rarity when drafting randomly

    [Header("Variants / Elites (optional)")]
    public List<Modifier> extraMods;     // add these to the piece on spawn
}

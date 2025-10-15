using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/Tile Data")]
public class TileData : ScriptableObject
{
    public string displayName;
    public Color tint = Color.white;
    public List<TileRule> tileRules; // affects pieces on/enter/leave this tile
}

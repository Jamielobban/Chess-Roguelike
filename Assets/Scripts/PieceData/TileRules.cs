using UnityEngine;

[CreateAssetMenu(menuName="RogueChess/TileRules/Lava")]
public class LavaRule : TileRule
{
    public int damage = 1;
    public override void OnEnter(Piece piece, BoardRuntime board, Tile2D tile)
    {
        piece.SendMessage("ReceiveDamage", damage, SendMessageOptions.DontRequireReceiver);
    }
}

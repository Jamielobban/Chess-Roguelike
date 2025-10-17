using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RuleEngine
{
    public static List<MoveOption> GetLegalMovesWithCosts(Piece p, BoardRuntime b)
        => MoveGenerator.GetMoves(p, b);

    public static void FireLeaveEnter(Piece p, BoardRuntime b, Vector2Int from, Vector2Int to)
        => MoveResolver.ExecuteMove(p, to, b);
}

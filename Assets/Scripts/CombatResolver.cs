// CombatResolver.cs
using UnityEngine;

public struct CombatOutcome
{
    public bool targetDied;
    public int damageDealt;
}

public static class CombatResolver
{
    // simple rewards; tweak or move into PieceData if you want per-piece tuning
    public static int xpOnHit  = 1;
    public static int xpOnKill = 3;

    public static CombatOutcome ResolveAttack(Piece attacker, Piece defender)
    {
        GameSignals.RaiseAttackStarted(attacker, defender.GridPos);

        int dmg = Mathf.Max(0, attacker.data.attack);
        defender.ReceiveDamage(dmg);
        GameSignals.RaiseAttackHit(attacker, defender, dmg);

        bool died = defender.HP <= 0;
        if (died)
        {
            //attacker.AddXP(xpOnKill);
            GameSignals.RaiseUnitKilled(attacker, defender);
        }
        else
        {
            //attacker.AddXP(xpOnHit);
        }

        return new CombatOutcome { targetDied = died, damageDealt = dmg };
    }
}

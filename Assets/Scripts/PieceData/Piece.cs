using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Piece : MonoBehaviour
{
    [Header("Data")]
    public PieceData data;                 // base stats, rules, base modifiers, tags

    [Header("Pos/Team")]
    public Vector2Int GridPos;
    public Team Team { get; private set; }

    [Header("Runtime Stats")]
    [SerializeField] private int _hp;      // current HP
    public int HP => _hp;
    public int MaxHP { get; private set; } // recomputed from data + modifiers
    public int Attack { get; private set; } // recomputed from data + modifiers

    [Header("Runtime Mods")]
    public List<Modifier> runtimeMods = new(); // stackable, added/removed at runtime

    // (Optional) XP/Level — enable if you want progression here
    // public int Level { get; private set; } = 1;
    // public int XP { get; private set; } = 0;

    private SpriteRenderer _sr;

    public void Init(PieceData d, Team? teamOverride = null)
    {
        data = d;
        Team = teamOverride ?? d.team;

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = d.sprite;
        _sr.color  = d.tint;

        // Start with base stats then apply modifiers
        RecalculateStats(initializing:true);

        name = $"{d.displayName} ({Team})";
    }

    /// Get a single stream of modifiers (base from data + runtime stacked)
    public IEnumerable<Modifier> AllModifiers()
    {
        if (data?.baseModifiers != null)
            foreach (var m in data.baseModifiers) if (m) yield return m;
        if (runtimeMods != null)
            foreach (var m in runtimeMods) if (m) yield return m;
    }

    /// Recompute Attack/MaxHP from data + all modifiers.
    public void RecalculateStats(bool initializing = false)
    {
        // Base stats
        int baseAtk = data ? data.attack : 1;
        int baseMax = data ? data.maxHP  : 1;

        // Apply modifiers
        int atk = baseAtk;
        int mx  = baseMax;
        foreach (var m in AllModifiers())
        {
            atk = m.ModifyAttack(this, atk);
            mx  = m.ModifyMaxHP(this, mx);
        }

        // Clamp minimums
        Attack = Mathf.Max(0, atk);
        int oldMax = MaxHP;
        MaxHP = Mathf.Max(1, mx);

        // Initialize / clamp HP
        if (initializing)
        {
            _hp = MaxHP;
        }
        else
        {
            // If max increased, you can choose to heal up to new max (design choice)
            // Example: keep current HP, just clamp to new max:
            _hp = Mathf.Clamp(_hp, 0, MaxHP);

            // Or, if you want to heal the difference when MaxHP increases:
            // if (MaxHP > oldMax) _hp += (MaxHP - oldMax);
            // _hp = Mathf.Clamp(_hp, 0, MaxHP);
        }

        // (Optional) notify UI
        GameSignals.RaiseHPChanged(this, _hp, _hp, MaxHP); // same old/new just to refresh bars
    }

    /// Attach/detach runtime modifiers safely
    public void AddModifier(Modifier mod, bool recalc = true)
    {
        if (mod == null) return;
        runtimeMods ??= new List<Modifier>();
        runtimeMods.Add(mod);
        mod.OnAttach(this);
        if (recalc) RecalculateStats();
    }

    public void RemoveModifier(Modifier mod, bool recalc = true)
    {
        if (mod == null || runtimeMods == null) return;
        if (runtimeMods.Remove(mod))
        {
            mod.OnDetach(this);
            if (recalc) RecalculateStats();
        }
    }

    // Damage/heal with signals so UI can react
    public void ReceiveDamage(int dmg)
    {
        int amount = Mathf.Max(0, dmg);
        int old = _hp;
        _hp = Mathf.Max(0, _hp - amount);
        GameSignals.RaiseHPChanged(this, old, _hp, MaxHP);

        foreach (var m in AllModifiers()) m.OnDamageTaken(this, amount);
    }

    public void Heal(int amt)
    {
        int amount = Mathf.Max(0, amt);
        int old = _hp;
        _hp = Mathf.Min(MaxHP, _hp + amount);
        GameSignals.RaiseHPChanged(this, old, _hp, MaxHP);
    }

    // (Optional) XP system — uncomment if needed
    // int XPNeededForNext() => 5 + (Level - 1) * 5;
    // public void AddXP(int amount)
    // {
    //     if (amount <= 0) return;
    //     int oldXP = XP;
    //     XP += amount;
    //     GameSignals.RaiseXPChanged(this, oldXP, XP);
    //     while (XP >= XPNeededForNext())
    //     {
    //         XP -= XPNeededForNext();
    //         Level++;
    //         GameSignals.RaiseLevelUp(this, Level);
    //         // On level up you could add a runtime modifier instead of mutating data
    //     }
    // }

    // Convenience
    public bool IsDead => _hp <= 0;
    //public IEnumerable<string> Tags => data?.tags ?? System.Array.Empty<string>();

    public void SetVisible(bool vis)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr) _sr.enabled = vis;
    }
}

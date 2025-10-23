using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Settings")]
    public int maxEnergyPerTurn = 6;
    public Team startingTeam = Team.White;

    [Header("Runtime")]
    public Team currentTeam { get; private set; }
    public int energy { get; private set; }

    public event Action<Team, int, int> OnTurnStarted;   // (team, energy, max)
    public event Action<int, int> OnEnergyChanged;      // (energy, max)
    public event Action<Team> OnTurnEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => BeginNewTurn(startingTeam);

    public void BeginNewTurn(Team team)
    {
        //Debug.Log(team);
        currentTeam = team;
        energy = maxEnergyPerTurn;
        OnTurnStarted?.Invoke(currentTeam, energy, maxEnergyPerTurn);
        OnEnergyChanged?.Invoke(energy, maxEnergyPerTurn);
        GameSignals.RaisePreviewEnergyChanged(energy, maxEnergyPerTurn);
    }

    public bool TrySpend(int cost)
    {
        if (cost < 0) cost = 0;
        if (energy < cost) return false;
        energy -= cost;
        OnEnergyChanged?.Invoke(energy, maxEnergyPerTurn);
        return true;
    }

    public void EndTurn()
    {
        var ended = currentTeam;
        currentTeam = (currentTeam == Team.White) ? Team.Black : Team.White;
        OnTurnEnded?.Invoke(ended);
        Debug.Log("Turn is now ->> " + currentTeam); 
        BeginNewTurn(currentTeam);
    }
}

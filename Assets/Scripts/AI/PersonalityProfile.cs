using UnityEngine;

public enum PersonalityPriority
{
    LethalCapture,
    Capture,
    Approach,
    Retreat
}

[CreateAssetMenu(menuName = "RogueChess/AI/Personality Profile")]
public class PersonalityProfile : ScriptableObject
{
    [Header("Turn Flow")]
    public Team team = Team.Black;
    public int   maxActionsPerTurn = 6;
    public float actionDelay = 0.25f;
    public float minScoreToAct = 0.0f; // gate; if best < this → end turn

    [Header("Heuristic Weights")]
    public float wLethalCapture = 120f;
    public float wCaptureDamage = 6f;
    public float wTargetValue   = 1.5f;
    public float wCloseDistance = 0.8f;   // approach strength
    public float wStride        = 2.0f;   // prefer longer strides per energy
    public float wEnergyCost    = 0.7f;   // penalize expensive moves
    public float wRepeat        = 1.0f;   // penalize moving same piece again
    public float wThreatPenalty = 1.0f;   // penalty per enemy that can hit our destination
    public float noiseStdDev    = 0.15f;

    [Header("Retreat Settings")]
    [Range(0f, 1f)] public float lowHPPercent = 0.33f; // below this → consider retreat
    public int threatRetreatThreshold = 2;             // retreat if >= this many threats

    [Header("Priority Order (top to bottom)")]
    public PersonalityPriority[] priorities = new[]
    {
        PersonalityPriority.LethalCapture,
        PersonalityPriority.Capture,
        PersonalityPriority.Approach,
        PersonalityPriority.Retreat
    };

    [Header("Telegraph Colors")]
    public Color originColor = new(1f, 0.3f, 0.3f, 1f);
    public Color pathColor   = new(0.6f, 0.8f, 1f, 1f);
    public Color destColor   = new(1f, 0.6f, 0.1f, 1f);
}

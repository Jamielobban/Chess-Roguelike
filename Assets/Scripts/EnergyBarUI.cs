using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class EnergyBarUI : MonoBehaviour
{
    [Header("Refs")]
    public Slider slider;                 // assign your Slider
    void Awake()
    {
        //if (!slider) slider = GetComponentInChildren<Slider>();
    }

    void OnEnable()
    {
        Debug.Log(TurnManager.Instance);
        if (TurnManager.Instance != null)
        {
            Debug.Log("tEST");
            TurnManager.Instance.OnTurnStarted   += HandleTurnStarted;
            TurnManager.Instance.OnEnergyChanged += HandleEnergyChanged;

            // initialize immediately if TurnManager already started
            HandleEnergyChanged(TurnManager.Instance.energy, TurnManager.Instance.maxEnergyPerTurn);
        }
    }

    void OnDisable()
    {
        var tm = TurnManager.Instance;
        if (tm != null)
        {
            tm.OnTurnStarted   -= HandleTurnStarted;
            tm.OnEnergyChanged -= HandleEnergyChanged;
        }
    }

    void HandleTurnStarted(Team team, int energy, int max)
    {
        Debug.Log(max);
        SetBar(energy, max);
    }

    void HandleEnergyChanged(int energy, int max)
    {
        Debug.Log(energy);
        SetBar(energy, max);
    }

    void SetBar(int energy, int max)
    {
        if (!slider) return;
        slider.maxValue = Mathf.Max(1, max);
        slider.value = Mathf.Clamp(energy, 0, max);

        //if (label)
        //    label.text = $"{energy} / {max}";
    }
}

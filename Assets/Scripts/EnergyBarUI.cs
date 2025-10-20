using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    public Slider currentSlider;   // light
    public Slider previewSlider;   // orange

    int _curMax = 1;
    int _curEnergy = 0;
    int _curPreview = 0;

    void OnEnable()
    {
        var tm = TurnManager.Instance;
        if (tm != null)
        {
            tm.OnTurnStarted   += HandleTurn;
            tm.OnEnergyChanged += HandleEnergy;
            // init
            HandleTurn(tm.currentTeam, tm.energy, tm.maxEnergyPerTurn);
        }

        GameSignals.OnPreviewEnergyChanged += HandlePreview;
    }

    void OnDisable()
    {
        var tm = TurnManager.Instance;
        if (tm != null)
        {
            tm.OnTurnStarted   -= HandleTurn;
            tm.OnEnergyChanged -= HandleEnergy;
        }
        GameSignals.OnPreviewEnergyChanged -= HandlePreview;
    }

    void HandleTurn(Team _, int energy, int max)
    {
        _curMax = Mathf.Max(1, max);
        _curEnergy = Mathf.Clamp(energy, 0, _curMax);
        _curPreview = _curEnergy;
        Apply();
    }

    void HandleEnergy(int energy, int max)
    {
        _curMax = Mathf.Max(1, max);
        _curEnergy = Mathf.Clamp(energy, 0, _curMax);
        Apply();
    }

    void HandlePreview(int preview, int max)
    {
        _curMax = Mathf.Max(1, max);
        _curPreview = Mathf.Clamp(preview, 0, _curMax);
        Apply();
    }

    void Apply()
    {
        if (currentSlider)
        {
            currentSlider.maxValue = _curMax;
            currentSlider.value = _curEnergy;
        }
        if (previewSlider)
        {
            previewSlider.maxValue = _curMax;
            previewSlider.value = _curPreview;
        }
    }
}

using UnityEngine;
using VContainer;

public class HealthUI : BaseStatBarUI
{
    private void Awake()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerHealthUpdated += UpdateDisplay;
        }
    }

    private void OnDestroy()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerHealthUpdated -= UpdateDisplay;
        }
    }
}
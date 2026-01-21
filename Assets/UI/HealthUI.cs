public class HealthUI : BaseStatBarUI
{
    private void OnEnable()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerHealthUpdated += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerHealthUpdated -= UpdateDisplay;
        }
    }
}
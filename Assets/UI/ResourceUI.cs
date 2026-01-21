public class ResourceUI : BaseStatBarUI
{
    private void OnEnable()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerResourceUpdated += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerResourceUpdated -= UpdateDisplay;
        }
    }
}
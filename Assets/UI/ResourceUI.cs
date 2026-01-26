public class ResourceUI : BaseStatBarUI
{
    private void Awake()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerResourceUpdated += UpdateDisplay;
        }
    }

    private void OnDestroy()
    {
        if (UiEventBus != null)
        {
            UiEventBus.PlayerResourceUpdated -= UpdateDisplay;
        }
    }
}
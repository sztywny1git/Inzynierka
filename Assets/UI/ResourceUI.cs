public class ResourceUI : BaseStatBarUI
{
    private void Start()
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
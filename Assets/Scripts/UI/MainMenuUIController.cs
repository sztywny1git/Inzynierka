using UnityEngine;
using VContainer;

public class MainMenuUIController : MonoBehaviour
{
    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
    }

    public void OnStartButtonPressed()
    {
        _uiEventBus.RequestStartGame();
    }
}
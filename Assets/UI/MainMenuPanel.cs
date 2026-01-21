using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button playButton;
    
    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
    }

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
    }

    private void OnPlayButtonPressed()
    {
        _uiEventBus.RequestStartNewGame();
    }
}
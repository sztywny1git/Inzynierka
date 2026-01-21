using UnityEngine;
using VContainer;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private RunSummaryScreen runSummaryScreen;

    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
        
        _uiEventBus.ShowLoadingScreenRequested += ShowLoading;
        _uiEventBus.HideLoadingScreenRequested += HideLoading;
        _uiEventBus.HUDVisibilityRequested += SetHUDVisibility;
        _uiEventBus.ShowRunSummaryRequested += ShowRunSummary;
        _uiEventBus.HideRunSummaryRequested += HideRunSummary;
    }

    private void Start()
    {
        if (runSummaryScreen != null)
        {
            runSummaryScreen.ReturnClicked += HandleRunSummaryReturnClicked;
            runSummaryScreen.Close();
        }

        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_uiEventBus != null)
        {
            
            _uiEventBus.ShowLoadingScreenRequested -= ShowLoading;
            _uiEventBus.HideLoadingScreenRequested -= HideLoading;
            _uiEventBus.HUDVisibilityRequested -= SetHUDVisibility;
            _uiEventBus.ShowRunSummaryRequested -= ShowRunSummary;
            _uiEventBus.HideRunSummaryRequested -= HideRunSummary;
        }

        if (runSummaryScreen != null)
        {
            runSummaryScreen.ReturnClicked -= HandleRunSummaryReturnClicked;
        }
    }

    private void HandleRunSummaryReturnClicked()
    {
        _uiEventBus.RequestExitRunSummary();
    }

    private void ShowRunSummary(RunOutcome outcome)
    {
        if (runSummaryScreen != null) runSummaryScreen.Open(outcome);
    }

    private void HideRunSummary()
    {
        if (runSummaryScreen != null) runSummaryScreen.Close();
    }

    private void ShowLoading()
    {
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    private void HideLoading()
    {
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
    }

    private void SetHUDVisibility(bool isVisible)
    {
        if (hudPanel != null) hudPanel.SetActive(isVisible);
    }
}
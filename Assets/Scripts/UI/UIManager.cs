using UnityEngine;
using VContainer;
using System;

public class UIManager : MonoBehaviour, IDisposable
{
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;

    private UIEventBus _uiEvents;

    private void Awake()
    {
        if (loadingScreenPanel == null) Debug.LogError("Loading Screen Panel is not assigned in UIManager!", this);
        if (gameOverPanel == null) Debug.LogError("Game Over Panel is not assigned in UIManager!", this);
        if (hudPanel == null) Debug.LogError("HUD Panel is not assigned in UIManager!", this);
    }

    [Inject]
    public void Construct(UIEventBus uiEvents)
    {
        _uiEvents = uiEvents;
        _uiEvents.OnShowGameOver += ShowGameOver;
        _uiEvents.OnShowLoadingScreen += ShowLoading;
        _uiEvents.OnHideLoadingScreen += HideLoading;
    }

    public void Dispose()
    {
        _uiEvents.OnShowGameOver -= ShowGameOver;
        _uiEvents.OnShowLoadingScreen -= ShowLoading;
        _uiEvents.OnHideLoadingScreen -= HideLoading;
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        hudPanel.SetActive(false);
    }

    private void ShowLoading()
    {
        loadingScreenPanel.SetActive(true);
    }

    private void HideLoading()
    {
        loadingScreenPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
    }
}
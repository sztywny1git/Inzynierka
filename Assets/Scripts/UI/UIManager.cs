using UnityEngine;
using VContainer;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour, IDisposable
{
    [Header("UI Panels")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject hudPanel;

    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
        _uiEventBus.ShowLoadingScreenRequested += ShowLoading;
        _uiEventBus.HideLoadingScreenRequested += HideLoading;
    }

    public void Dispose()
    {
        if (_uiEventBus != null)
        {
            _uiEventBus.ShowLoadingScreenRequested -= ShowLoading;
            _uiEventBus.HideLoadingScreenRequested -= HideLoading;
        }
    }

    private void Start()
    {
        loadingScreenPanel.SetActive(false);
        hudPanel.SetActive(false);
    }

    private void ShowLoading()
    {
        loadingScreenPanel.SetActive(true);
    }

    private void HideLoading()
    {
        loadingScreenPanel.SetActive(false);
        hudPanel.SetActive(true);
    }
}
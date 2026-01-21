using UnityEngine;
using UnityEngine.UI;
using System;

public class RunSummaryScreen : MonoBehaviour
{
    [SerializeField] private Button _returnToHubButton;
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;

    public event Action ReturnClicked;

    private void Awake()
    {
        if (_returnToHubButton != null)
        {
            _returnToHubButton.onClick.AddListener(() => ReturnClicked?.Invoke());
        }
    }

    public void Open(RunOutcome outcome)
    {
        gameObject.SetActive(true);
        if (_victoryPanel != null) _victoryPanel.SetActive(outcome == RunOutcome.Victory);
        if (_defeatPanel != null) _defeatPanel.SetActive(outcome == RunOutcome.Defeat);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
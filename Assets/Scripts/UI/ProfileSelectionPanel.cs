using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using VContainer;
using System;

public class ProfileSelectionPanel : MonoBehaviour, IDisposable
{
    [SerializeField] private List<ProfileSlotUI> profileSlots;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject mainPanel;

    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
        _uiEventBus.ProfileListUpdated += UpdateDisplay;
    }
    
    public void Dispose()
    {
        _uiEventBus.ProfileListUpdated -= UpdateDisplay;
    }

    private void Awake()
    {
        backButton.onClick.AddListener(Hide);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void UpdateDisplay(List<ProfileSummary> summaries)
    {
        for (int i = 0; i < profileSlots.Count; i++)
        {
            if (i < summaries.Count)
            {
                var summary = summaries[i];
                var slotUI = profileSlots[i];
                
                slotUI.Refresh(summary);
                
                slotUI.GetSelectButton().onClick.RemoveAllListeners();

                slotUI.GetSelectButton().onClick.AddListener(() => _uiEventBus.SelectProfile(summary.SlotId));

                slotUI.GetDeleteButton().onClick.RemoveAllListeners();
                slotUI.GetDeleteButton().onClick.AddListener(() => OnDeleteButtonPressed(summary.SlotId));
            }
        }
    }
    
    private void OnDeleteButtonPressed(int slotId)
    {
        // TODO: Pokaż okno "Confirm Delete?" i wywołaj to dopiero po potwierdzeniu
        _uiEventBus.RequestProfileDeletion(slotId);
    }
}
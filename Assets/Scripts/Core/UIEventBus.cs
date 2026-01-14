using System;
using UnityEngine;
using System.Collections.Generic;

public class UIEventBus
{
    

    public event Action ShowRunSummaryRequestd;
    public void RequestShowRunSummary() => ShowRunSummaryRequestd?.Invoke();

    public event Action ShowLoadingScreenRequested;
    public void RequestLoadingScreen() => ShowLoadingScreenRequested?.Invoke();

    public event Action HideLoadingScreenRequested;
    public void RequestHideLoadingScreen() => HideLoadingScreenRequested?.Invoke();
    //moze lepiej zrobic loading screen jako przełączanie(bool)
        
    public event Action<float, float> PlayerHealthUpdated;
    public void PublishPlayerHealthUpdate(float current, float max) => PlayerHealthUpdated?.Invoke(current, max);
    
    public event Action<string> InteractionTooltipUpdated;
    public void UpdateInteractionTooltip(string text) => InteractionTooltipUpdated?.Invoke(text);

    public event Action PauseMenuToggleRequested;
    public void RequestTogglePauseMenu() => PauseMenuToggleRequested?.Invoke();

    public event Action<List<ProfileSummary>> ProfileListUpdated;
    public void PublishProfileList(List<ProfileSummary> summaries) => ProfileListUpdated?.Invoke(summaries);

    public event Action<int> ProfileSelected;
    public void SelectProfile(int slotId) => ProfileSelected?.Invoke(slotId);

    public event Action<int> ProfileDeletionRequested;
    public void RequestProfileDeletion(int slotId) => ProfileDeletionRequested?.Invoke(slotId);
}
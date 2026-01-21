using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class UIEventBus
{
    public event Action<RunOutcome> ShowRunSummaryRequested;
    public void RequestShowRunSummary(RunOutcome outcome) => ShowRunSummaryRequested?.Invoke(outcome);

    public event Action HideRunSummaryRequested;
    public void RequestHideRunSummary() => HideRunSummaryRequested?.Invoke();

    public event Action ExitRunSummaryRequested;
    public void RequestExitRunSummary() => ExitRunSummaryRequested?.Invoke();

    public event Action ShowLoadingScreenRequested;
    public void RequestLoadingScreen() => ShowLoadingScreenRequested?.Invoke();

    public event Action HideLoadingScreenRequested;
    public void RequestHideLoadingScreen() => HideLoadingScreenRequested?.Invoke();

    public event Action<bool> HUDVisibilityRequested;
    public void RequestHUDVisibility(bool isVisible) => HUDVisibilityRequested?.Invoke(isVisible);

    public event Action<float, float> PlayerHealthUpdated;
    public void PublishPlayerHealthUpdate(float current, float max) => PlayerHealthUpdated?.Invoke(current, max);

    public event Action<float, float> PlayerResourceUpdated;
    public void PublishPlayerResourceUpdate(float current, float max) => PlayerResourceUpdated?.Invoke(current, max);

    public event Action<string> InteractionTooltipUpdated;
    public void UpdateInteractionTooltip(string text) => InteractionTooltipUpdated?.Invoke(text);

    public event Action PauseMenuToggleRequested;
    public void RequestTogglePauseMenu() => PauseMenuToggleRequested?.Invoke();

    public event Action StartNewGameRequested;
    public void RequestStartNewGame() => StartNewGameRequested?.Invoke();

    public event Func<UniTask> FadeInRequested;
    public async UniTask RequestFadeIn()
    {
        if (FadeInRequested != null) await FadeInRequested.Invoke();
    }

    public event Func<UniTask> FadeOutRequested;
    public async UniTask RequestFadeOut()
    {
        if (FadeOutRequested != null) await FadeOutRequested.Invoke();
    }
}
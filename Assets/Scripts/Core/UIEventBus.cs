using System;

public class UIEventBus
{
    public event Action OnRequestStartGame;
    public void RequestStartGame() => OnRequestStartGame?.Invoke();

    public event Action OnShowGameOver;
    public void RequestGameOverScreen() => OnShowGameOver?.Invoke();

    public event Action OnShowLoadingScreen;
    public void RequestLoadingScreen() => OnShowLoadingScreen?.Invoke();

    public event Action OnHideLoadingScreen;
    public void RequestHideLoadingScreen() => OnHideLoadingScreen?.Invoke();

    public event Action<float, float> OnUpdateHealth;
    public void UpdateHealth(float current, float max) => OnUpdateHealth?.Invoke(current, max);
}
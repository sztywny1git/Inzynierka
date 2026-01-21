using Cysharp.Threading.Tasks;
using System;

public class RunSummaryState : IGameState, IDisposable
{
    private readonly UIEventBus _uiEvents;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionData;
    private readonly TimeService _timeService;

    public RunSummaryState(
        UIEventBus uiEvents, 
        GameplayEventBus gameplayEvents,
        SessionData sessionData,
        TimeService timeService)
    {
        _uiEvents = uiEvents;
        _gameplayEvents = gameplayEvents;
        _sessionData = sessionData;
        _timeService = timeService;
    }

    public async UniTask OnEnter()
    {
        _timeService.RequestPause(this);
        
        _uiEvents.ExitRunSummaryRequested += HandleExitRequested;
        
        _uiEvents.RequestHUDVisibility(false);
        _uiEvents.RequestShowRunSummary(_sessionData.CurrentRunOutcome);
        
        await UniTask.CompletedTask;
    }

    public UniTask OnExit()
    {
        _uiEvents.ExitRunSummaryRequested -= HandleExitRequested;
        _uiEvents.RequestHideRunSummary();
        _timeService.ReleasePause(this);
        return UniTask.CompletedTask;
    }

    public void Dispose()
    {
        _uiEvents.ExitRunSummaryRequested -= HandleExitRequested;
    }

    private void HandleExitRequested()
    {
        _uiEvents.ExitRunSummaryRequested -= HandleExitRequested;
        _gameplayEvents.RequestGoToHub();
    }
}
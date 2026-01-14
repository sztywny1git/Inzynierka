using Cysharp.Threading.Tasks;
using UnityEngine;
public class RunSummaryState : IGameState
{
    private readonly UIEventBus _uiEvents;
    private readonly GameScopeService _scopeService;
    public RunSummaryState(UIEventBus uiEvents, GameScopeService scopeService)
    {
        _uiEvents = uiEvents;
        _scopeService = scopeService;
    }

    public UniTask OnEnter()
    {
        Time.timeScale = 0;
        _uiEvents.RequestShowRunSummary();
        return UniTask.CompletedTask;
    }
    public UniTask OnExit()
    {
        Time.timeScale = 1;
        return UniTask.CompletedTask;
    }
}
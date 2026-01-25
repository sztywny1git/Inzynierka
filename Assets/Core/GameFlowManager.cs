using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

public class GameFlowManager : IInitializable, IDisposable
{
    private readonly GameplayEventBus _gameplayEvents;
    private readonly ScreenTransitionService _transitionService;
    private readonly Dictionary<GameStateId, IGameState> _states;
    private IGameState _currentState;

    public GameFlowManager(
        GameplayEventBus gameplayEvents,
        ScreenTransitionService transitionService,
        MainMenuState mainMenuState,
        HubState hubState,
        RunState runState,
        RunSummaryState runSummaryState)
    {
        _gameplayEvents = gameplayEvents;
        _transitionService = transitionService;

        _states = new Dictionary<GameStateId, IGameState>
        {
            { GameStateId.MainMenu, mainMenuState },
            { GameStateId.Hub, hubState },
            { GameStateId.Run, runState },
            { GameStateId.RunSummary, runSummaryState },
        };
        
        _gameplayEvents.GoToHubRequested += OnGoToHubRequested;
        _gameplayEvents.BeginRunRequested += OnBeginRunRequested;
        _gameplayEvents.EndRunRequested += OnEndRunRequested;
    }

    public void Initialize()
    {
        ChangeStateInternal(GameStateId.MainMenu).Forget();
    }

    public void Dispose()
    {
        _gameplayEvents.GoToHubRequested -= OnGoToHubRequested;
        _gameplayEvents.BeginRunRequested -= OnBeginRunRequested;
        _gameplayEvents.EndRunRequested -= OnEndRunRequested;
    }

    private void OnGoToHubRequested() => TransitionToState(GameStateId.Hub).Forget();
    private void OnBeginRunRequested() => TransitionToState(GameStateId.Run).Forget();
    private void OnEndRunRequested() => TransitionToState(GameStateId.RunSummary).Forget();

    private async UniTaskVoid TransitionToState(GameStateId newStateId)
    {
        await _transitionService.PerformTransition(async () =>
        {
            await ChangeStateInternal(newStateId);
        });
    }

    private async UniTask ChangeStateInternal(GameStateId newStateId)
    {
        if (_currentState != null)
        {
            await _currentState.OnExit();
        }

        _currentState = _states[newStateId];
        await _currentState.OnEnter();
    }
}
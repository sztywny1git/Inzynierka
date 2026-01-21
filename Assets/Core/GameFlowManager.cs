using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

public class GameFlowManager : IInitializable, IDisposable
{
    private readonly IObjectResolver _container;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly ScreenTransitionService _transitionService;

    private Dictionary<GameStateId, IGameState> _states;
    private IGameState _currentState;

    public GameFlowManager(
        IObjectResolver container, 
        GameplayEventBus gameplayEvents,
        ScreenTransitionService transitionService)
    {
        _container = container;
        _gameplayEvents = gameplayEvents;
        _transitionService = transitionService;
        
        _gameplayEvents.GoToHubRequested += OnGoToHubRequested;
        _gameplayEvents.BeginRunRequested += OnBeginRunRequested;
        _gameplayEvents.EndRunRequested += OnEndRunRequested;
    }

    public void Initialize()
    {
        InitializeStates();
        ChangeStateInternal(GameStateId.MainMenu).Forget();
    }

    public void Dispose()
    {
        _gameplayEvents.GoToHubRequested -= OnGoToHubRequested;
        _gameplayEvents.BeginRunRequested -= OnBeginRunRequested;
        _gameplayEvents.EndRunRequested -= OnEndRunRequested;
    }

    private void InitializeStates()
    {
        _states = new Dictionary<GameStateId, IGameState>
        {
            { GameStateId.MainMenu, _container.Resolve<MainMenuState>() },
            { GameStateId.Hub, _container.Resolve<HubState>() },
            { GameStateId.Run, _container.Resolve<RunState>() },
            { GameStateId.RunSummary, _container.Resolve<RunSummaryState>() },
        };
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
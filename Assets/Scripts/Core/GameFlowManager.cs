using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;

public class GameFlowManager : IInitializable, IDisposable
{
    private readonly IObjectResolver _container;
    private readonly GameplayEventBus _gameplayEvents;

    private Dictionary<GameStateId, IGameState> _states;
    private IGameState _currentState;

    public GameFlowManager(
        IObjectResolver container, 
        GameplayEventBus gameplayEvents, 
        UIEventBus uiEvents)
    {
        _container = container;
        _gameplayEvents = gameplayEvents;
        
        _gameplayEvents.GoToHubRequested += () => ChangeState(GameStateId.Hub);
        _gameplayEvents.BeginRunRequested += () => ChangeState(GameStateId.Run);
        _gameplayEvents.EndRunRequested += () => ChangeState(GameStateId.RunSummary);
    }

    public void Initialize()
    {
        InitializeStates();
        ChangeState(GameStateId.MainMenu);
    }

    public void Dispose()
    {
        _gameplayEvents.GoToHubRequested -= () => ChangeState(GameStateId.Hub);
        _gameplayEvents.BeginRunRequested -= () => ChangeState(GameStateId.Run);
        _gameplayEvents.EndRunRequested -= () => ChangeState(GameStateId.RunSummary);
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

    public async void ChangeState(GameStateId newStateId)
    {
        if (_currentState != null)
        {
            await _currentState.OnExit();
        }

        _currentState = _states[newStateId];
        await _currentState.OnEnter();
    }
    

}
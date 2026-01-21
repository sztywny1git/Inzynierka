using System.Threading.Tasks;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

public class MainMenuState : IGameState
{
    private readonly ISceneContextManager _sceneManager;
    private readonly UIEventBus _uiEventBus;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionData;
    private readonly GameConstants _gameConstants;
    private readonly LifetimeScope _gameScope;

    public MainMenuState(
        ISceneContextManager sceneManager, 
        UIEventBus uiEventBus,
        GameplayEventBus gameplayEvents,
        SessionData sessionData,
        GameConstants gameConstants,
        LifetimeScope gameScope)
    {
        _sceneManager = sceneManager;
        _uiEventBus = uiEventBus;
        _gameplayEvents = gameplayEvents;
        _sessionData = sessionData;
        _gameConstants = gameConstants;
        _gameScope = gameScope;
    }
    
    public async UniTask OnEnter()
    {
        _uiEventBus.StartNewGameRequested += OnStartNewGameRequested;

        using (LifetimeScope.EnqueueParent(_gameScope))
        {
            await _sceneManager.LoadSceneAsync("MainMenu");
        }
    }

    public UniTask OnExit()
    {
        _uiEventBus.StartNewGameRequested -= OnStartNewGameRequested;
        return UniTask.CompletedTask;
    }

    private void OnStartNewGameRequested()
    {
        _sessionData.CurrentPlayerClass = _gameConstants.DefaultPlayerClass;
        _gameplayEvents.RequestGoToHub();
    }
}
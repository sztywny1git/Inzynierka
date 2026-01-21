using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

public class RunState : IGameState, IDisposable
{
    private readonly GameScopeService _scopeService;
    private readonly ISceneContextManager _sceneManager;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionData;
    private readonly ScreenTransitionService _transitionService;
    private readonly GameConstants _gameConstants;

    public RunState(
        GameScopeService scopeService,
        ISceneContextManager sceneManager,
        GameplayEventBus gameplayEvents,
        SessionData sessionData,
        ScreenTransitionService transitionService,
        GameConstants gameConstants)
    {
        _scopeService = scopeService;
        _sceneManager = sceneManager;
        _gameplayEvents = gameplayEvents;
        _sessionData = sessionData;
        _transitionService = transitionService;
        _gameConstants = gameConstants;
    }

    public async UniTask OnEnter()
    {
        _sessionData.ResetRunData();

        _gameplayEvents.LevelLoadRequested += HandleLoadLevel;
        _gameplayEvents.RunWon += HandleRunWon;
        _gameplayEvents.PlayerDied += HandlePlayerDied;

        _scopeService.DestroyActiveScope();
        var scope = await _scopeService.CreateGameplayScope();
        
        var progressionManager = scope.Container.Resolve<RunProgressionManager>();
        progressionManager.StartRun();
    }

    public UniTask OnExit()
    {
        UnsubscribeEvents();
        return UniTask.CompletedTask;
    }

    public void Dispose()
    {
        UnsubscribeEvents();
    }
    
    private void UnsubscribeEvents()
    {
        _gameplayEvents.LevelLoadRequested -= HandleLoadLevel;
        _gameplayEvents.RunWon -= HandleRunWon;
        _gameplayEvents.PlayerDied -= HandlePlayerDied;
    }

    private void HandleLoadLevel()
    {
        LoadProceduralScene().Forget();
    }

    private void HandlePlayerDied() => EndRunSequence(RunOutcome.Defeat);
    private void HandleRunWon() => EndRunSequence(RunOutcome.Victory);

    private void EndRunSequence(RunOutcome outcome)
    {
        _sessionData.CurrentRunOutcome = outcome;
        _gameplayEvents.RequestEndRun();
    }

    private async UniTaskVoid LoadProceduralScene()
    {
        await _transitionService.PerformTransition(async () =>
        {
             var scope = _scopeService.GetActiveScope();
             using (LifetimeScope.EnqueueParent(scope))
             {
                 await _sceneManager.LoadSceneAsync("ProceduralLevel");
             }
        });
    }
}
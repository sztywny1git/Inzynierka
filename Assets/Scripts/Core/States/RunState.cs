using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class RunState : IGameState, IDisposable
{
    private readonly GameScopeService _scopeService;
    private readonly ISceneContextManager _sceneManager;
    private readonly IResourceLoader _resourceLoader;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionData;
    private readonly UIEventBus _uiEvents;

    public RunState(
        GameScopeService scopeService,
        ISceneContextManager sceneManager,
        IResourceLoader resourceLoader,
        GameplayEventBus gameplayEvents,
        SessionData sessionData,
        UIEventBus uiEvents)
    {
        _scopeService = scopeService;
        _sceneManager = sceneManager;
        _resourceLoader = resourceLoader;
        _gameplayEvents = gameplayEvents;
        _sessionData = sessionData;
        _uiEvents = uiEvents;
    }

    public async UniTask OnEnter()
    {
        _gameplayEvents.LevelLoadRequested += HandleLoadLevel;
        _gameplayEvents.RunWon += HandleRunWon;
        _gameplayEvents.PlayerDied += HandlePlayerDied;

        _scopeService.DestroyActiveScope();
        var scope = await _scopeService.CreateGameplayScope();
        
        var runDef = _resourceLoader.Load<RunDefinition>("Data/Level/RunDefinition");
        
        if (runDef == null)
        {
            Debug.LogError("Failed to load RunDefinition!");
            return;
        }
        var progressionManager = scope.Container.Resolve<RunProgressionManager>();
        progressionManager.StartRun(runDef);
        await UniTask.CompletedTask;
    }

    public UniTask OnExit()
    {
        UnsubscribeEvents();
        _sessionData.ResetRunData();
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

    private void HandleLoadLevel(LevelDefinition levelDef)
    {
        _sessionData.CurrentLevelDefinition = levelDef;
        _ = LoadProceduralScene();
    }

    private void HandlePlayerDied()
    {
        EndRun(RunOutcome.Defeat);
    }

    private void HandleRunWon()
    {
        EndRun(RunOutcome.Victory);
    }

    private void EndRun(RunOutcome outcome)
    {
        Debug.Log($"RunState: Ending Run -> {outcome}");
        _sessionData.CurrentRunOutcome = outcome;
        _gameplayEvents.RequestEndRun();
    }

    private async UniTask LoadProceduralScene()
    {
        _uiEvents.RequestLoadingScreen();
        
        var scope = _scopeService.GetActiveScope();
        using (LifetimeScope.EnqueueParent(scope))
        {
            await _sceneManager.LoadSceneAsync("ProceduralLevel");
        }
        _uiEvents.RequestHideLoadingScreen();
    }
}
using System;
using System.Threading.Tasks;
using VContainer.Unity;
using UnityEngine;

public class GameFlowManager : IInitializable, IDisposable
{
    private readonly ISceneContextManager _sceneManager;
    private readonly GameScopeService _scopeService;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly UIEventBus _uiEvents;

    public GameFlowManager(
        ISceneContextManager sceneManager, 
        GameScopeService scopeService,
        GameplayEventBus gameplayEvents,
        UIEventBus uiEvents)
    {
        _sceneManager = sceneManager;
        _scopeService = scopeService;
        _gameplayEvents = gameplayEvents;
        _uiEvents = uiEvents;
        _gameplayEvents.OnCharacterDied += HandleCharacterDied;
        _uiEvents.OnRequestStartGame += HandleStartGameRequested;
    }

    public void Initialize() { }

    public void Dispose()
    {
        _gameplayEvents.OnCharacterDied -= HandleCharacterDied;
        _uiEvents.OnRequestStartGame -= HandleStartGameRequested;
    }

    private void HandleStartGameRequested()
    {
        _ = EnterHub();
    }

    private void HandleCharacterDied(Character deadCharacter)
    {
        if (deadCharacter.CompareTag("Player"))
        {
            _uiEvents.RequestGameOverScreen();
            _ = EndCurrentRunAndReturnToHub();
        }
    }

    private async Task EndCurrentRunAndReturnToHub()
    {
        await Task.Delay(3000); 
        await EnterHub();
    }

    public async Task EnterHub()
    {
        _uiEvents.RequestLoadingScreen();
        _scopeService.DestroyActiveScope();

        var gameplayScope = _scopeService.CreateGameplayScope();
        
        using (LifetimeScope.EnqueueParent(gameplayScope))
        {
            await _sceneManager.LoadSceneAsync("Hub");
        }

        _uiEvents.RequestHideLoadingScreen();
    }

    public async Task StartNewRun()
    {
        _uiEvents.RequestLoadingScreen();
        _scopeService.DestroyActiveScope();
        var gameplayScope = _scopeService.CreateGameplayScope();
        
        using (LifetimeScope.EnqueueParent(gameplayScope))
        {
            await _sceneManager.LoadSceneAsync("ProceduralLevel");
        }

        _uiEvents.RequestHideLoadingScreen();
    }

    public async Task GoToNextLevel(string nextSceneName)
    {
        _uiEvents.RequestLoadingScreen();
        
        var activeScope = _scopeService.GetActiveScope();
        using (LifetimeScope.EnqueueParent(activeScope))
        {
            await _sceneManager.LoadSceneAsync(nextSceneName);
        }

        _uiEvents.RequestHideLoadingScreen();
    }
}
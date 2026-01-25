using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class RunState : IGameState, IDisposable
{
    private readonly GameScopeService _scopeService;
    private readonly ISceneContextManager _sceneManager;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionData;
    private readonly ScreenTransitionService _transitionService;

    public RunState(
        GameScopeService scopeService,
        ISceneContextManager sceneManager,
        GameplayEventBus gameplayEvents,
        SessionData sessionData,
        ScreenTransitionService transitionService)
    {
        _scopeService = scopeService;
        _sceneManager = sceneManager;
        _gameplayEvents = gameplayEvents;
        _sessionData = sessionData;
        _transitionService = transitionService;
    }

    public async UniTask OnEnter()
    {
        _sessionData.ResetRunData();

        _gameplayEvents.RunWon += HandleRunWon;
        _gameplayEvents.PlayerDied += HandlePlayerDied;
        _gameplayEvents.LevelLoadRequested += HandleLevelLoadRequested;

        _scopeService.DestroyActiveScope();
        var scope = await _scopeService.CreateGameplayScope();
        
        var progressionManager = scope.Container.Resolve<RunProgressionManager>();
        
        var levelReadyTask = WaitForLevelReady();

        progressionManager.StartRun();

        await levelReadyTask;
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
        _gameplayEvents.RunWon -= HandleRunWon;
        _gameplayEvents.PlayerDied -= HandlePlayerDied;
        _gameplayEvents.LevelLoadRequested -= HandleLevelLoadRequested;
    }

    private void HandleLevelLoadRequested()
    {
        LoadLevelSequence().Forget();
    }

    private async UniTaskVoid LoadLevelSequence()
    {
        await _transitionService.FadeInAsync();

        var currentScene = SceneManager.GetSceneByName("ProceduralLevel");
        if (currentScene.IsValid() && currentScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(currentScene);
        }

        await _sceneManager.LoadSceneAsync("ProceduralLevel");

        var initializer = Object.FindFirstObjectByType<ProceduralSceneInitializer>();
        if (initializer != null)
        {
            var levelReadyTask = WaitForLevelReady();
            
            await initializer.GenerateLevel();
            
            await levelReadyTask; 
        }
        else
        {
            Debug.LogError("ProceduralSceneInitializer not found in ProceduralLevel scene!");
        }

        await _transitionService.FadeOutAsync();
    }

    private async UniTask WaitForLevelReady()
    {
        var completionSource = new UniTaskCompletionSource();
        
        void OnLevelReady(Transform _) => completionSource.TrySetResult();

        _gameplayEvents.LevelReady += OnLevelReady;
        
        await completionSource.Task;
        
        _gameplayEvents.LevelReady -= OnLevelReady;
    }

    private void HandlePlayerDied() => EndRunSequence(RunOutcome.Defeat);
    private void HandleRunWon() => EndRunSequence(RunOutcome.Victory);

    private void EndRunSequence(RunOutcome outcome)
    {
        _sessionData.CurrentRunOutcome = outcome;
        _gameplayEvents.RequestEndRun();
    }
}
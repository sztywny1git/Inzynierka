using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class HubState : IGameState
{
    private readonly GameScopeService _scopeService;
    private readonly ISceneContextManager _sceneManager;
    private readonly GameplayEventBus _gameplayEvents;

    public HubState(
        GameScopeService scopeService, 
        ISceneContextManager sceneManager,
        GameplayEventBus gameplayEvents)
    {
        _scopeService = scopeService;
        _sceneManager = sceneManager;
        _gameplayEvents = gameplayEvents;
    }

    public async UniTask OnEnter()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetInventory();
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.ResetEquipment();
        }

        if (AugmentManager.Instance != null)
        {
            AugmentManager.Instance.ResetAugments();
        }

        _scopeService.DestroyActiveScope();
        var scope = await _scopeService.CreateGameplayScope();

        var levelReadyTask = WaitForLevelReady();

        using (LifetimeScope.EnqueueParent(scope))
        {
            await _sceneManager.LoadSceneAsync("Hub");
        }

        await levelReadyTask;
    }

    public UniTask OnExit()
    {
        return UniTask.CompletedTask;
    }

    private async UniTask WaitForLevelReady()
    {
        var completionSource = new UniTaskCompletionSource();
        
        void OnLevelReady(Transform _) => completionSource.TrySetResult();

        _gameplayEvents.LevelReady += OnLevelReady;
        
        await completionSource.Task;
        
        _gameplayEvents.LevelReady -= OnLevelReady;
    }
}
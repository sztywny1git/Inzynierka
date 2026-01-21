using Cysharp.Threading.Tasks;
using VContainer.Unity;
using UnityEngine;
public class HubState : IGameState
{
    private readonly GameScopeService _scopeService;
    private readonly ISceneContextManager _sceneManager;
    public HubState(GameScopeService scopeService, ISceneContextManager sceneManager)
    {
        _scopeService = scopeService;
        _sceneManager = sceneManager;
    }

    public async UniTask OnEnter()
    {

        var scope = await _scopeService.CreateGameplayScope();
        using (LifetimeScope.EnqueueParent(scope))
        {
            await _sceneManager.LoadSceneAsync("Hub");
        }
    }
    public UniTask OnExit()
    {
        _scopeService.DestroyActiveScope();
        return UniTask.CompletedTask;
    }
}
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameBootstrapper : IAsyncStartable
{
    private readonly ISceneContextManager _sceneManager;
    private readonly LifetimeScope _parentScope;

    public GameBootstrapper(ISceneContextManager sceneManager, LifetimeScope parentScope)
    {
        _sceneManager = sceneManager;
        _parentScope = parentScope;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        using (LifetimeScope.EnqueueParent(_parentScope))
        {
            await _sceneManager.LoadSceneAsync("MainMenu");
        }
    }
}
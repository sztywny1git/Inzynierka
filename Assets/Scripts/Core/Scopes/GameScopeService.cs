using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
public class GameScopeService
{
    private readonly IResourceLoader _resourceLoader;
    private readonly LifetimeScope _rootScope;
    private LifetimeScope _currentScope;

    public GameScopeService(IResourceLoader resourceLoader, LifetimeScope rootScope)
    {
        _resourceLoader = resourceLoader;
        _rootScope = rootScope;
    }

    public UniTask<LifetimeScope> CreateGameplayScope()
    {
        if (_currentScope != null) return UniTask.FromResult(_currentScope);

        var scopePrefab = _resourceLoader.LoadPrefab("Prefabs/Scopes/GameplayScope");

        using (LifetimeScope.EnqueueParent(_rootScope))
        {
            var scopeInstance = Object.Instantiate(scopePrefab);    
            
            Object.DontDestroyOnLoad(scopeInstance);
            _currentScope = scopeInstance.GetComponent<LifetimeScope>();
        }

        return UniTask.FromResult(_currentScope);
    }

    public void DestroyActiveScope()
    {
        if (_currentScope != null)
        {
            Object.Destroy(_currentScope.gameObject);
            _currentScope = null;
        }
    }

    public LifetimeScope GetActiveScope()
    {
        return _currentScope;
    }
}
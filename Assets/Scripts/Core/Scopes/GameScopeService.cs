using UnityEngine;
using VContainer.Unity;

public class GameScopeService
{
    private readonly IResourceLoader _resourceLoader;
    private LifetimeScope _currentScope;

    public GameScopeService(IResourceLoader resourceLoader)
    {
        _resourceLoader = resourceLoader;
    }

    public LifetimeScope CreateGameplayScope()
    {
        if (_currentScope != null) return _currentScope;

        var scopePrefab = _resourceLoader.LoadPrefab("Prefabs/Scopes/GameplayScope");
        var scopeInstance = Object.Instantiate(scopePrefab);
        Object.DontDestroyOnLoad(scopeInstance);
        _currentScope = scopeInstance.GetComponent<LifetimeScope>();
        return _currentScope;
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
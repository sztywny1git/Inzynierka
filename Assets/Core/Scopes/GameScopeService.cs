using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

public class GameScopeService
{
    private readonly LifetimeScope _rootScope;
    private LifetimeScope _currentScope;

    public GameScopeService(LifetimeScope rootScope)
    {
        _rootScope = rootScope;
    }

    public UniTask<LifetimeScope> CreateGameplayScope()
    {
        if (_currentScope != null)
        {
            DestroyActiveScope();
        }

        _currentScope = _rootScope.CreateChild<GameplayScope>();

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
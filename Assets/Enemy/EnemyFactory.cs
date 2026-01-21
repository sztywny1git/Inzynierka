using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EnemyFactory : IEnemyFactory
{
    private readonly IObjectResolver _container;
    private readonly EnemyRegistry _enemyRegistry;

    public EnemyFactory(IObjectResolver container, EnemyRegistry enemyRegistry)
    {
        _container = container;
        _enemyRegistry = enemyRegistry;
    }

    public GameObject CreateEnemy(EnemyType type, Vector3 at)
    {
        GameObject enemyPrefab = _enemyRegistry.GetPrefab(type);
        
        if(enemyPrefab == null) return null;

        var enemyInstance = _container.Instantiate(enemyPrefab, at, Quaternion.identity);
        return enemyInstance;
    }
}
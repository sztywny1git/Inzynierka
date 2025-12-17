using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EnemyFactory : IEnemyFactory
{
    private readonly IObjectResolver _container;
    private readonly IResourceLoader _resourceLoader;

    public EnemyFactory(IObjectResolver container, IResourceLoader resourceLoader)
    {
        _container = container;
        _resourceLoader = resourceLoader;
    }

    public GameObject CreateEnemy(EnemyType type, Vector3 at)
    {
        GameObject enemyPrefab = LoadPrefabFor(type);
        var enemyInstance = _container.Instantiate(enemyPrefab, at, Quaternion.identity);
        return enemyInstance;
    }

    private GameObject LoadPrefabFor(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Goblin:
                return _resourceLoader.LoadPrefab("Entities/Enemies/Goblin");
            case EnemyType.Skeleton:
                return _resourceLoader.LoadPrefab("Entities/Enemies/Skeleton");
            default:
                throw new System.ArgumentException("Invalid enemy type specified: " + type);
        }
    }
}
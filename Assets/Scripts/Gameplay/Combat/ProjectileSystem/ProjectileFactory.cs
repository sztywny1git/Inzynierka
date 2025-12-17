using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectileFactory : IProjectileFactory
{
    private readonly IObjectResolver _container;
    private readonly IResourceLoader _resourceLoader;

    public ProjectileFactory(IObjectResolver container, IResourceLoader resourceLoader)
    {
        _container = container;
        _resourceLoader = resourceLoader;
    }

    public ProjectileBase Spawn(
        GameObject owner,
        string prefabPath,
        Vector3 position,
        IProjectileMovementStrategy movementStrategy,
        float damage,
        int pierceCount)
    {
        var prefab = _resourceLoader.LoadPrefab(prefabPath);
        if (prefab == null) return null;

        var go = _container.Instantiate(prefab, position, Quaternion.identity);
        
        var projectile = go.GetComponent<ProjectileBase>();
        if (projectile != null)
        {
            projectile.Initialize(owner, movementStrategy, damage, pierceCount);
        }

        var visual = go.GetComponent<ProjectileVisual>();
        if (visual != null)
        {
            visual.Initialize(projectile);
        }

        return projectile;
    }
}
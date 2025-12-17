using UnityEngine;

public interface IProjectileFactory
{
    ProjectileBase Spawn(
        GameObject owner,
        string prefabPath,
        Vector3 position,
        IProjectileMovementStrategy movementStrategy,
        float damage,
        int pierceCount
    );
}
using UnityEngine;

public interface IProjectileMovementStrategy
{
    void Initialize(ProjectileBase projectile);
    
    void Move();

    bool IsMovementDone { get; }
}
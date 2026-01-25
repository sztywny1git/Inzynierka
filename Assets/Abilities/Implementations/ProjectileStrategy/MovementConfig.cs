using UnityEngine;

public abstract class ProjectileMovementConfig : ScriptableObject
{
    public abstract IMovementStrategy InitializeStrategy(IMovementStrategy existing, Vector3 start, Vector3 target, float speed);
}
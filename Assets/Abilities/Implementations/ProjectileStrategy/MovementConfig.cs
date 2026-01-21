using UnityEngine;

public abstract class ProjectileMovementConfig : ScriptableObject
{
    public abstract IMovementStrategy CreateStrategy(
        Vector3 origin, 
        Quaternion rotation, 
        Vector3 targetPos, 
        float finalSpeed
    );
}
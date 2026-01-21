using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Movement/Linear")]
public class LinearMovementConfig : ProjectileMovementConfig
{
    public override IMovementStrategy CreateStrategy(Vector3 origin, Quaternion rotation, Vector3 targetPos, float finalSpeed)
    {
        return new LinearMovementStrategy(finalSpeed);
    }
}

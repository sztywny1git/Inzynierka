using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Movement/Homing")]
public class HomingMovementConfig : ProjectileMovementConfig
{
    [SerializeField] private float _turnSpeed = 200f;
    [SerializeField] private float _searchRadius = 10f;
    [SerializeField] private LayerMask _targetLayer;

    public override IMovementStrategy CreateStrategy(Vector3 origin, Quaternion rotation, Vector3 targetPos, float finalSpeed)
    {
        return new HomingMovementStrategy(finalSpeed, _turnSpeed, _searchRadius, _targetLayer);
    }
}
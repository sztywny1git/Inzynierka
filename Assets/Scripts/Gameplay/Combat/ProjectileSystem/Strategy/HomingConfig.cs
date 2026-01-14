using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Movement/Homing")]
public class HomingMovementConfig : ProjectileMovementConfig
{
    [SerializeField] private float _turnSpeed = 200f;
    [SerializeField] private float _searchRadius = 10f;
    [SerializeField] private LayerMask _targetLayer;

    public override IMovementStrategy CreateStrategy(Vector3 origin, Quaternion rotation, Vector3 targetPos, float finalSpeed)
    {
        Collider2D hit = Physics2D.OverlapCircle(origin, _searchRadius, _targetLayer);
        Transform targetTransform = hit != null ? hit.transform : null;

        return new HomingMovementStrategy(targetTransform, finalSpeed, _turnSpeed);
    }
}
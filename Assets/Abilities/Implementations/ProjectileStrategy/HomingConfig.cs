using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Homing Movement")]
public class HomingMovementConfig : ProjectileMovementConfig
{
    [SerializeField] private float _turnSpeed = 200f;
    [SerializeField] private float _searchRadius = 10f;
    [SerializeField] private LayerMask _targetLayer;

    public override IMovementStrategy InitializeStrategy(IMovementStrategy existing, Vector3 start, Vector3 target, float speed)
    {
        HomingMovementStrategy strategy = existing as HomingMovementStrategy;
        
        if (strategy == null) strategy = new HomingMovementStrategy();

        strategy.Reset(speed, _turnSpeed, _searchRadius, _targetLayer);
        return strategy;
    }
}
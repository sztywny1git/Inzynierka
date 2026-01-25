using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Linear Movement")]
public class LinearMovementConfig : ProjectileMovementConfig
{
    public override IMovementStrategy InitializeStrategy(IMovementStrategy existing, Vector3 start, Vector3 target, float speed)
    {
        LinearMovementStrategy strategy = existing as LinearMovementStrategy;
        
        if (strategy == null) strategy = new LinearMovementStrategy();
        
        strategy.Reset(speed);
        return strategy;
    }
}

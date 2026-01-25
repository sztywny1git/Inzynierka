using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Arc Movement")]
public class ArcMovementConfig : ProjectileMovementConfig
{
    [SerializeField] private float _arcHeight = 2f;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _defaultDistance = 10f;

    public override IMovementStrategy InitializeStrategy(IMovementStrategy existing, Vector3 start, Vector3 target, float speed)
    {
        float distance = Vector3.Distance(start, target);
        if (distance < 0.1f) distance = _defaultDistance;

        float duration = distance / Mathf.Max(speed, 0.1f);

        ArcMovementStrategy strategy = existing as ArcMovementStrategy;
        
        if (strategy == null) strategy = new ArcMovementStrategy();

        strategy.Reset(target, duration, _arcHeight, _curve);
        return strategy;
    }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Movement/Arc")]
public class ArcMovementConfig : ProjectileMovementConfig
{
    [SerializeField] private float _arcHeight = 2f;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _defaultDistance = 10f;

    public override IMovementStrategy CreateStrategy(Vector3 origin, Quaternion rotation, Vector3 targetPos, float finalSpeed)
    {
        float distance = Vector3.Distance(origin, targetPos);
        if (distance < 0.1f) distance = _defaultDistance;

        float duration = distance / Mathf.Max(finalSpeed, 0.1f);

        return new ArcMovementStrategy(targetPos, duration, _arcHeight, _curve);
    }
}
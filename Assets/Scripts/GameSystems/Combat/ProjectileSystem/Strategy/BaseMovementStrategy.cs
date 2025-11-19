// Plik: ArcMovementStrategy.cs
using UnityEngine;

public class BaseMovementStrategy : IProjectileMovementStrategy
{
    private ProjectileBase projectile;
    private Transform transform;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyTravelTime;
    private float absoluteMaxHeight;
    private AnimationCurve trajectoryCurve;
    
    private float journeyTimer;
    
    public bool IsMovementDone => journeyTimer >= journeyTravelTime;

    public BaseMovementStrategy(Vector3 target, float duration, float maxHeight, AnimationCurve curve)
    {
        this.targetPosition = target;
        this.journeyTravelTime = duration;
        this.absoluteMaxHeight = maxHeight;
        this.trajectoryCurve = curve;
    }

    public void Initialize(ProjectileBase projectile)
    {
        this.projectile = projectile;
        this.transform = projectile.transform;
        this.startPosition = this.transform.position;
        this.journeyTimer = 0f;
    }

    public void Move()
    {
        journeyTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(journeyTimer / journeyTravelTime);

        Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);

        if (trajectoryCurve != null)
        {
            float arcHeight = trajectoryCurve.Evaluate(progress);
            currentPosition.y += arcHeight * absoluteMaxHeight;
        }
        
        transform.position = currentPosition;
    }
}
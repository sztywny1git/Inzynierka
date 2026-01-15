using UnityEngine;

/// <summary>
/// Context object containing all references needed by boss states.
/// </summary>
public class BossContext
{
    public BossController Controller { get; }
    public Transform Transform { get; }
    public BossAnimator Animator { get; }
    public Health Health { get; }
    
    // Target tracking
    public Transform Target { get; private set; }
    
    // Phase management
    public int CurrentPhase { get; private set; }
    public const int TotalPhases = 2;
    
    public BossContext(
        BossController controller,
        BossAnimator animator,
        Health health)
    {
        Controller = controller;
        Transform = controller.transform;
        Animator = animator;
        Health = health;
        CurrentPhase = 1;
    }
    
    public void SetTarget(Transform target)
    {
        Target = target;
    }
    
    public void SetPhase(int phase)
    {
        CurrentPhase = Mathf.Clamp(phase, 1, TotalPhases);
    }
    
    public float GetHealthPercentage()
    {
        if (Health == null) return 1f;
        return Health.CurrentHealth / Health.MaxHealth;
    }
    
    public bool IsTargetInRange(float range)
    {
        if (Target == null) return false;
        return Vector3.Distance(Transform.position, Target.position) <= range;
    }
    
    public Vector3 GetDirectionToTarget()
    {
        if (Target == null) return Vector3.zero;
        return (Target.position - Transform.position).normalized;
    }
    
    public float GetDistanceToTarget()
    {
        if (Target == null) return float.MaxValue;
        return Vector3.Distance(Transform.position, Target.position);
    }
}

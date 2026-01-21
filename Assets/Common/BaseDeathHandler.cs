using UnityEngine;

[RequireComponent(typeof(IHealthProvider))]
public abstract class BaseDeathHandler : MonoBehaviour
{
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float safetyFallbackDuration = 2.0f;
    
    protected IHealthProvider HealthProvider;
    protected Animator Animator;
    
    private Collider2D[] _colliders;
    private Rigidbody2D _rigidbody;
    private bool _isDeathFinalized;

    protected virtual void Awake()
    {
        HealthProvider = GetComponent<IHealthProvider>();
        Animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _colliders = GetComponentsInChildren<Collider2D>();
    }

    protected virtual void OnEnable()
    {
        if (HealthProvider != null)
            HealthProvider.Death += HandleDeath;
    }

    protected virtual void OnDisable()
    {
        if (HealthProvider != null)
            HealthProvider.Death -= HandleDeath;
    }

    protected virtual void HandleDeath()
    {
        if (_isDeathFinalized) return;

        foreach (var col in _colliders)
        {
            col.enabled = false;
        }

        if (_rigidbody != null)
        {
            _rigidbody.simulated = false;
        }

        if (Animator != null)
        {
            Animator.SetTrigger(deathTriggerName);
        }

        Invoke(nameof(ForceFinishDeath), safetyFallbackDuration);
    }

    public virtual void OnDeathAnimationFinished()
    {
        if (_isDeathFinalized) return;
        
        _isDeathFinalized = true;
        CancelInvoke(nameof(ForceFinishDeath));
    }

    private void ForceFinishDeath()
    {
        OnDeathAnimationFinished();
    }
}
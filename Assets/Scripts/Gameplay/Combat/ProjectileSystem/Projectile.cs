using UnityEngine;
using System; // Potrzebne do Action
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    private IMovementStrategy _movementStrategy;
    private Action<GameObject> _returnToPoolAction;
    private GameObject _owner;
    
    private DamageData _damageData;
    
    private int _pierceCount;
    private float _lifetime;
    private int _hitCount;
    
    private readonly List<int> _hitTargets = new List<int>();
    private TrailRenderer _trailRenderer;
    private Rigidbody2D _rb2d;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _rb2d = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Action<GameObject> returnToPoolAction,
        GameObject owner,
        IMovementStrategy strategy,
        DamageData damageData,
        int pierceCount,
        float lifetime)
    {
        _returnToPoolAction = returnToPoolAction;
        _owner = owner;
        _movementStrategy = strategy;
        _damageData = damageData;
        _pierceCount = pierceCount;
        _lifetime = lifetime;
        
        _hitCount = 0;
        _hitTargets.Clear();

        if (_rb2d != null)
        {
            _rb2d.linearVelocity = Vector2.zero;
            _rb2d.angularVelocity = 0f;
        }

        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
        }

        _movementStrategy.Initialize(transform);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        
        if (_movementStrategy != null)
        {
            _movementStrategy.Update(dt);
            
            if (_movementStrategy.IsDone)
            {
                DespawnSelf();
                return;
            }
        }

        _lifetime -= dt;
        if (_lifetime <= 0)
        {
            DespawnSelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == _owner) return;

        int targetId = other.GetInstanceID();
        if (_hitTargets.Contains(targetId)) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            DamageData finalPayload = _damageData;
            finalPayload.SourcePosition = transform.position;

            damageable.TakeDamage(finalPayload);
            
            _hitTargets.Add(targetId);
            _hitCount++;

            if (_hitCount > _pierceCount)
            {
                DespawnSelf();
            }
        }
        else if (!other.isTrigger) 
        {
            DespawnSelf();
        }
    }

    private void DespawnSelf()
    {
        _returnToPoolAction?.Invoke(gameObject);
    }
}
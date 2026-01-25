using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : PoolableObject
{
    [SerializeField] private VfxObject _hitVfxPrefab; 

    private GameObject _instigator;
    private IMovementStrategy _movementStrategy;
    private DamageData _damageData;
    private IAbilitySpawner _spawner; 
    
    private int _remainingPierce;
    private HashSet<GameObject> _hitHistory = new HashSet<GameObject>();
    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public void Initialize(
        GameObject instigator, 
        ProjectileMovementConfig movementConfig, 
        Vector3 targetPos,
        float speed,
        DamageData damageData, 
        int pierceCount, 
        float lifetime, 
        IAbilitySpawner spawner)
    {
        _instigator = instigator;
        _damageData = damageData;
        _remainingPierce = pierceCount;
        _spawner = spawner; 

        if (movementConfig != null)
        {
            _movementStrategy = movementConfig.InitializeStrategy(
                _movementStrategy, 
                transform.position, 
                targetPos, 
                speed
            );
            
            _movementStrategy.Initialize(transform);
        }

        SetLifetime(lifetime);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        _hitHistory.Clear();
        _collider.enabled = true;
    }

    protected override void Update()
    {
        if (_movementStrategy != null)
        {
            _movementStrategy.Update(Time.deltaTime);
            
            if (_movementStrategy.IsDone)
            {
                SpawnVfx(transform.position);
                ReturnToPool();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == _instigator) return;
        if (_hitHistory.Contains(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damageData);
            _hitHistory.Add(other.gameObject);
            
            SpawnVfx(other.ClosestPoint(transform.position));

            if (_remainingPierce < 0) return;

            if (_remainingPierce > 0)
            {
                _remainingPierce--;
            }
            else
            {
                ReturnToPool();
            }
        }
        else 
        {
            SpawnVfx(other.ClosestPoint(transform.position));
            ReturnToPool();
        }
    }

    private void SpawnVfx(Vector2 position)
    {
        if (_hitVfxPrefab != null && _spawner != null)
        {
            _spawner.Spawn(_hitVfxPrefab, position, Quaternion.identity);
        }
    }
}
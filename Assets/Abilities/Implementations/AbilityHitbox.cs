using System.Collections.Generic;
using UnityEngine;

public class AbilityHitbox : PoolableObject
{
    [SerializeField] private float _defaultLifetime = 0.5f;
    [SerializeField] private bool _destroyOnImpact = false;
    [SerializeField] private VfxObject _hitVfxPrefab;

    private DamageData _damageData;
    private IAbilitySpawner _spawner;
    private readonly HashSet<GameObject> _hitTargets = new HashSet<GameObject>();

    public void Initialize(DamageData damageData, IAbilitySpawner spawner)
    {
        _damageData = damageData;
        _spawner = spawner;
        SetLifetime(_defaultLifetime);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        _hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitTargets.Contains(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damageData);
            _hitTargets.Add(other.gameObject);
            
            SpawnVfx(other.ClosestPoint(transform.position));

            if (_destroyOnImpact)
            {
                ReturnToPool();
            }
        }
    }

    private void SpawnVfx(Vector2 position)
    {
        if (_hitVfxPrefab != null && _spawner != null)
        {
            Vector2 direction = (position - (Vector2)transform.position).normalized;
            
            if (direction == Vector2.zero) direction = transform.right;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion vfxRotation = Quaternion.Euler(0f, 0f, angle);

            _spawner.Spawn(_hitVfxPrefab, position, vfxRotation);
        }
    }
}
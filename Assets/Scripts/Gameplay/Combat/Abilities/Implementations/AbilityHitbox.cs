using UnityEngine;
using System.Collections.Generic;

public class AbilityHitbox : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;
    [SerializeField] private bool _destroyOnImpact = false;
    
    private DamageData _damageData;
    private bool _initialized = false;
    private HashSet<GameObject> _hitTargets = new HashSet<GameObject>();

    public void Initialize(DamageData damageData)
    {
        _damageData = damageData;
        _initialized = true;
        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        if (_hitTargets.Contains(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damageData);
            _hitTargets.Add(other.gameObject);

            if (_destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}
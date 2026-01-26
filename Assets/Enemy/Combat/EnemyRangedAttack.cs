using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Attack Settings")]
    [SerializeField] private float damage = 8f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float attackCooldownSeconds = 1.25f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetimeSeconds = 3f;

    [Header("Origin")]
    [SerializeField] private Transform shootOrigin;

    private float _nextAttackTime;
    private Transform _currentTarget; 

    public float AttackRange => attackRange;

    private void Awake()
    {
        if (shootOrigin == null) shootOrigin = transform;
        _nextAttackTime = 0f;
    }

    public bool CanAttackNow()
    {
        return Time.time >= _nextAttackTime;
    }

    public void PrepareAttack(Transform target)
    {
        if (!CanAttackNow()) return;

        _currentTarget = target;
        _nextAttackTime = Time.time + attackCooldownSeconds;
    }

    public void OnAnimationShootEvent()
    {
        if (_currentTarget == null) return;
        
        SpawnProjectile(_currentTarget);
    }

    private void SpawnProjectile(Transform target)
    {
        if (projectilePrefab == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[EnemyRangedAttack] '{name}' SpawnProjectile failed: projectilePrefab=null.", this);
            }
            return;
        }

        Vector2 dir = (target.position - shootOrigin.position);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        var instance = Instantiate(projectilePrefab, shootOrigin.position, Quaternion.identity);
        var enemyProjectile = instance.GetComponent<EnemyProjectile>() ?? instance.GetComponentInChildren<EnemyProjectile>();
        
        if (enemyProjectile != null)
        {
            enemyProjectile.Initialize(dir, damage, projectileSpeed, projectileLifetimeSeconds, instance, gameObject);
        }
        else
        {
            var rb = instance.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
            }

            Destroy(instance, projectileLifetimeSeconds);
        }

        if (debugLogging)
        {
            Debug.Log($"[EnemyRangedAttack] '{name}' SHOT at '{target.name}'.", this);
        }
    }
}
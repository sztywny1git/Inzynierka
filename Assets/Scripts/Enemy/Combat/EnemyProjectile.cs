using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetimeSeconds = 3f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Damage")]
    [Tooltip("Used only if Initialize() was not called (e.g., prefab misconfigured).")]
    [SerializeField] private float defaultDamageIfNotInitialized = 0f;

    private float _damage;
    private Vector2 _direction;
    private float _dieAt;

    private GameObject _destroyTarget;
    private GameObject _owner;

    private Rigidbody2D _rb;

    public void Initialize(Vector2 direction, float damage, float projectileSpeed, float lifetime)
    {
        Initialize(direction, damage, projectileSpeed, lifetime, null);
    }

    public void Initialize(Vector2 direction, float damage, float projectileSpeed, float lifetime, GameObject destroyTarget)
    {
        Initialize(direction, damage, projectileSpeed, lifetime, destroyTarget, null);
    }

    public void Initialize(Vector2 direction, float damage, float projectileSpeed, float lifetime, GameObject destroyTarget, GameObject owner)
    {
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        _damage = damage;
        speed = projectileSpeed;
        lifetimeSeconds = lifetime;

        // If the script is on a child, destroy the whole spawned projectile instance.
        _destroyTarget = destroyTarget != null ? destroyTarget : gameObject;
        _owner = owner;

        _dieAt = Time.time + lifetimeSeconds;

        if (_rb != null)
        {
            _rb.linearVelocity = _direction * speed;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>() ?? GetComponentInParent<Rigidbody2D>();
        _destroyTarget = gameObject;
        _dieAt = Time.time + lifetimeSeconds;

        if (Mathf.Abs(_damage) < 0.0001f && defaultDamageIfNotInitialized > 0f)
        {
            _damage = defaultDamageIfNotInitialized;
        }

        if (_rb != null)
        {
            _rb.gravityScale = 0f;
        }
    }

    private void Update()
    {
        if (Time.time >= _dieAt)
        {
            Destroy(_destroyTarget);
            return;
        }

        // Fallback movement if prefab has no Rigidbody2D.
        if (_rb == null)
        {
            // Move the whole projectile object even if the script is on a child.
            var t = _destroyTarget != null ? _destroyTarget.transform : transform;
            t.position += (Vector3)(_direction * (speed * Time.deltaTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other, isTrigger: true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        HandleHit(collision.collider, isTrigger: false);
    }

    private void HandleHit(Collider2D other, bool isTrigger)
    {
        if (other == null) return;

        // Ignore self-hits.
        if (other.transform == transform || other.transform.IsChildOf(transform)) return;

        // Ignore hits on the owner (the enemy that spawned this projectile).
        if (_owner != null && (other.gameObject == _owner || other.transform.IsChildOf(_owner.transform))) return;

        if (debugLogging)
        {
            Debug.Log($"[EnemyProjectile] '{name}' hit '{other.name}' (trigger={isTrigger}). damage={_damage:0.###} layer={LayerMask.LayerToName(other.gameObject.layer)}", this);
        }

        // Apply damage if possible.
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            if (debugLogging)
            {
                Debug.Log($"[EnemyProjectile] '{name}' found IDamageable on '{other.name}'. Calling TakeDamage({_damage:0.###}).", this);
            }
            var damageData = new DamageData(_damage, false, _owner, transform.position);
            damageable.TakeDamage(damageData);
            if (destroyOnHit) Destroy(_destroyTarget);
            return;
        }

        var parentDamageable = other.GetComponentInParent<IDamageable>();
        if (parentDamageable != null)
        {
            if (debugLogging)
            {
                var parentGo = (parentDamageable as Component)?.gameObject;
                string parentName = parentGo != null ? parentGo.name : "?";
                Debug.Log($"[EnemyProjectile] '{name}' found IDamageable on parent '{parentName}' of '{other.name}'. Calling TakeDamage({_damage:0.###}).", this);
            }
            var damageData = new DamageData(_damage, false, _owner, transform.position);
            parentDamageable.TakeDamage(damageData);
            if (destroyOnHit) Destroy(_destroyTarget);
            return;
        }

        if (debugLogging)
        {
            Debug.LogWarning($"[EnemyProjectile] '{name}' hit '{other.name}' but found NO IDamageable on it or any parent. Check if Health component exists and is on the same object or a parent of the collider.", this);
        }

        // If we hit anything, destroy (optional).
        if (destroyOnHit)
        {
            Destroy(_destroyTarget);
        }
    }
}

using UnityEngine;

/// <summary>
/// A projectile that moves in a straight line and damages the player on contact.
/// Used for boss ring attacks.
/// </summary>
public class BossProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask damageLayer;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color projectileColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    
    private Vector2 _direction;
    private float _spawnTime;
    
    public void Initialize(Vector2 direction, float speed, float damage, LayerMask targetLayer)
    {
        _direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.damageLayer = targetLayer;
        _spawnTime = Time.time;
        
        // Rotate sprite to face direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                CreateVisual();
            }
        }
    }
    
    private void CreateVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateProjectileSprite();
        spriteRenderer.color = projectileColor;
        spriteRenderer.sortingOrder = 50;
    }
    
    private Sprite CreateProjectileSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float alpha = 1f - (dist / radius) * 0.5f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 2);
    }
    
    private void Update()
    {
        // Move
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        
        // Lifetime check
        if (Time.time - _spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit target layer
        if (((1 << other.gameObject.layer) & damageLayer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var damageData = new DamageData(damage, false, gameObject, transform.position);
                damageable.TakeDamage(damageData);
            }
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _direction * 2f);
    }
}

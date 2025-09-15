using UnityEngine;

public class Bullet : MonoBehaviour
{

    private float speed;
    private float lifetime;
    private int damage;
    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        if (GetComponent<Collider2D>() == null)
        {
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.1f;
        }

        // Pobranie wartości z StatsManager w Awake
        if (StatsManager.Instance != null)
        {
            speed = StatsManager.Instance.bulletSpeed;
            lifetime = StatsManager.Instance.bulletLifetime;
            damage = StatsManager.Instance.bulletDamage;
        }
        else
        {
            Debug.LogWarning("StatsManager not found! Using fallback values for bullet.");
        }
    }


    private void Start()
    {
        // Ustawienie automatycznego zniszczenia pocisku po czasie życia
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Ustaw prędkość dopiero gdy mamy już speed ze Start()
        if (speed > 0)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // Jeśli Start() się jeszcze nie wykonał, użyj fallback
            Debug.LogWarning("StatsManager not found! Using fallback values for bullet.");
        }

        // Obracanie pocisku w kierunku lotu
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log($"Bullet direction set: {direction}, velocity: {rb.linearVelocity}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return; // Ignoruj kolizje z graczem
        }

        Debug.Log($"Bullet hit: {other.name}");

        /* Sprawdź czy trafił w wroga
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        }*/

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Bullet collided with: {collision.gameObject.name}");
        Destroy(gameObject);
    }
}
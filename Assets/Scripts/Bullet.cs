using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;
    
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
    }
    
    private void Start()
    {
        // Automatyczne zniszczenie pocisku po określonym czasie
        Destroy(gameObject, lifetime);
    }
    
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;
        
        // Obracanie pocisku w kierunku lotu
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sprawdź czy pocisk trafił w coś innego niż gracza
        if (other.CompareTag("Player"))
        {
            return; // Ignoruj kolizje z graczem
        }
        
        /* Sprawdź czy trafił w wroga (można dodać tag "Enemy")
        if (other.CompareTag("Enemy"))
        {
            // Zadaj obrażenia wrogowi, jeżeli posiada komponent EnemyHealth
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
        }*/
        
        // Zniszcz pocisk po trafieniu
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Zniszcz pocisk po kolizji z czymkolwiek
        Destroy(gameObject);
    }
}


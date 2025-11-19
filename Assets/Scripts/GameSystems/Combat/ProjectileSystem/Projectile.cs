using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 3f;

    void Start()
    {
        // Destroy projectile after its lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Apply damage logic here (e.g., if(other.TryGetComponent<Enemy>(out var enemy)) enemy.TakeDamage(damage))
        
        // Destroy projectile on collision
        Destroy(gameObject);
    }
}

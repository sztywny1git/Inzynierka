// Plik: HomingMovementStrategy.cs
using UnityEngine;

public class HomingMovementStrategy : IProjectileMovementStrategy
{
    private ProjectileBase projectile;
    private Transform transform;

    // Parametry Homing
    private Transform target;
    private float speed;
    private float turnSpeed;
    private float lifetime;

    public bool IsMovementDone => lifetime <= 0 || target == null || !target.gameObject.activeInHierarchy;

    public HomingMovementStrategy(Transform target, float speed, float turnSpeed, float maxLifetime)
    {
        this.target = target;
        this.speed = speed;
        this.turnSpeed = turnSpeed;
        this.lifetime = maxLifetime;
    }

    public void Initialize(ProjectileBase projectile)
    {
        this.projectile = projectile;
        this.transform = projectile.transform;
    }

    public void Move()
    {
        lifetime -= Time.deltaTime;
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // Płynny obrót w stronę celu
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget); // Dla 2D
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Ruch do przodu
        transform.position += transform.up * speed * Time.deltaTime; // W 2D 'up' to przód
    }
}
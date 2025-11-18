using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Attacks/Mage Basic")]
public class MageBasicAttack : ClassAttackBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    public override void Attack(Transform origin, Vector2 direction)
    {
        if (projectilePrefab == null) return;

        // Spawn projectile at origin
        var projectile = Instantiate(projectilePrefab, origin.position, Quaternion.identity);

        // Set projectile scale
        projectile.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // Set projectile velocity
        var rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction.normalized * projectileSpeed;
    }
}

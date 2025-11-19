using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 3f;
    public float patrolSpeed = 2f;

    [Header("Combat")]
    public Transform player;
    public float detectionRange = 5f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    private Transform firePoint;

    private Vector2 patrolStartPos;
    private Vector2 leftPoint;
    private Vector2 rightPoint;
    private bool goingRight = true;

    private Rigidbody2D rb;
    private Animator anim;
    private float fireCooldown = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        patrolStartPos = rb.position;
        leftPoint = patrolStartPos + Vector2.left * patrolDistance;
        rightPoint = patrolStartPos + Vector2.right * patrolDistance;

        GameObject fp = new GameObject("FirePoint");
        fp.transform.parent = transform;
        fp.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        firePoint = fp.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            ChaseAndShoot();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("isWalking", true);

        Vector2 target = goingRight ? rightPoint : leftPoint;
        Vector2 moveDir = (target - rb.position).normalized;
        rb.linearVelocity = moveDir * patrolSpeed;

        if (Vector2.Distance(rb.position, target) < 0.1f)
        {
            goingRight = !goingRight;
            Flip(moveDir.x);
        }
    }

    void ChaseAndShoot()
    {
        anim.SetBool("isWalking", false);
        rb.linearVelocity = Vector2.zero;

        float dirX = player.position.x - rb.position.x;
        Flip(dirX);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        anim.SetTrigger("attack");

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();

        Vector2 dir = (player.position - firePoint.position).normalized;
        projRb.linearVelocity = dir * projectileSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Flip(float moveX)
    {
        if ((moveX > 0 && transform.localScale.x < 0) || (moveX < 0 && transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

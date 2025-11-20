using UnityEngine;

public class GoblinMeleeAI : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolDistance = 3f;
    public float patrolSpeed = 2f;

    [Header("Chase")]
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public Transform player;

    [Header("Attack")]
    public float attackRange = 1f; 
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    private Vector2 patrolStartPos;
    private Vector2 leftPoint;
    private Vector2 rightPoint;
    private bool goingRight = true;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        patrolStartPos = transform.position;
        leftPoint = patrolStartPos + Vector2.left * patrolDistance;
        rightPoint = patrolStartPos + Vector2.right * patrolDistance;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer < detectionRange)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("attack");

        if (Time.time > lastAttackTime + attackCooldown)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.SetCurrentHealth(playerStats.CurrentHealth - attackDamage);
                
            }
            lastAttackTime = Time.time;
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
            Flip();
        }
    }

    void Chase()
    {
        anim.SetBool("isWalking", true);

        Vector2 moveDir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = moveDir * chaseSpeed;

        if ((moveDir.x > 0 && transform.localScale.x < 0) || (moveDir.x < 0 && transform.localScale.x > 0))
        {
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

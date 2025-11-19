using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;

    private Vector3 currentTarget;
    private Animator anim;

    void Start()
    {
        currentTarget = pointB.position;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance < detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("isWalking", true);

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = (currentTarget == pointA.position) ? pointB.position : pointA.position;
            Flip();
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime);

        if (player.position.x > transform.position.x && transform.localScale.x < 0)
            Flip();
        else if (player.position.x < transform.position.x && transform.localScale.x > 0)
            Flip();
    }

    void Flip()
    {
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }
}

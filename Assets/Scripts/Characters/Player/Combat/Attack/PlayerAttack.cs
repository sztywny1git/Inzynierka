using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerAttack : MonoBehaviour
{
    public Transform attackOrigin;
    public ClassAttackBehaviour attackBehaviour;
    public ProjectileFactory projectileFactory;
    private PlayerStats playerStats;
    private Animator animator;
    private bool isAttacking = false;

    // Minimum attack speed to prevent zero-speed animation
    private const float MinAttackAnimSpeed = 0.1f;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        projectileFactory = ProjectileFactory.Instance; 
    }

    void Update()
    {
        // Start attack on left mouse click
        if (Input.GetMouseButton(0) && !isAttacking)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            // Set AttackSpeed parameter based on player stats
            float attackSpeed = Mathf.Max(MinAttackAnimSpeed, playerStats.AttackSpeed.FinalValue);
            animator.SetFloat("AttackSpeed", attackSpeed);

            // Trigger attack animation
            animator.SetTrigger("Attack");
        }
        else
        {
            isAttacking = false;
        }
    }

    // Called via Animation Event during attack
    public void PerformAttack()
    {
        if (attackBehaviour == null || attackOrigin == null || projectileFactory == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)attackOrigin.position).normalized;

        attackBehaviour.Attack(attackOrigin, direction, playerStats, projectileFactory);
    }

    // Called via Animation Event at the end of attack
    public void FinishAttack()
    {
        isAttacking = false;

        if (animator != null)
        {
            // Reset attack speed to default (optional)
            animator.SetFloat("AttackSpeed", 1f);
        }
    }
}

using UnityEngine;

/// <summary>
/// Sub-state for the Summoner's ring attack.
/// Fires projectiles in a circle around the boss.
/// </summary>
public class SummonerRingAttackSubState : BossBaseSubState
{
    private readonly SummonerBossController _bossController;
    private readonly int _projectileCount;
    private readonly float _projectileSpeed;
    private readonly float _projectileDamage;
    
    private RingAttackIndicator _ringIndicator;
    private float _attackTimer;
    private bool _hasFired;
    private float _startAngle;
    
    private const float WarningDuration = 0.8f; // Time to show warning before firing
    private const float AttackDuration = 1.2f;  // Total attack duration
    
    public SummonerRingAttackSubState(
        BossContext context, 
        BossPhaseState parentPhase,
        SummonerBossController controller,
        int projectileCount = 8,
        float projectileSpeed = 4f,
        float projectileDamage = 15f) 
        : base(context, parentPhase)
    {
        _bossController = controller;
        _projectileCount = projectileCount;
        _projectileSpeed = projectileSpeed;
        _projectileDamage = projectileDamage;
    }
    
    public override void Enter()
    {
        base.Enter();
        _attackTimer = 0f;
        _hasFired = false;
        
        // Random starting angle for variety
        _startAngle = Random.Range(0f, 360f / _projectileCount);
        
        // Get or create ring indicator
        _ringIndicator = _bossController.GetComponent<RingAttackIndicator>();
        if (_ringIndicator == null)
        {
            _ringIndicator = _bossController.gameObject.AddComponent<RingAttackIndicator>();
        }
        
        // Show warning
        _ringIndicator.ShowRingWarning(_projectileCount, _startAngle, WarningDuration);
        
        // Play summon/cast animation (reuse summon for now)
        if (Context.Animator is SummonerBossAnimator summonerAnim)
        {
            summonerAnim.PlaySummon();
        }
        
        Debug.Log("[SummonerBoss] Preparing ring attack!");
    }
    
    public override void Tick(float deltaTime)
    {
        _attackTimer += deltaTime;
        
        // Fire projectiles after warning duration
        if (!_hasFired && _attackTimer >= WarningDuration)
        {
            _hasFired = true;
            FireProjectiles();
            _ringIndicator?.HideWarning();
        }
        
        // Complete after full duration
        if (_attackTimer >= AttackDuration)
        {
            Complete();
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        _ringIndicator?.HideWarning();
    }
    
    private void FireProjectiles()
    {
        float angleStep = 360f / _projectileCount;
        
        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = _startAngle + (angleStep * i);
            Vector2 direction = AngleToDirection(angle);
            
            SpawnProjectile(direction);
        }
        
        Debug.Log($"[SummonerBoss] Fired {_projectileCount} projectiles!");
    }
    
    private void SpawnProjectile(Vector2 direction)
    {
        // Create projectile
        GameObject projectileObj = new GameObject("BossProjectile");
        projectileObj.transform.position = Context.Transform.position;
        
        // Set layer - use "EnemyProjectile" if exists, otherwise use Default
        int projectileLayer = LayerMask.NameToLayer("EnemyProjectile");
        if (projectileLayer < 0 || projectileLayer > 31)
        {
            projectileLayer = LayerMask.NameToLayer("Default");
        }
        projectileObj.layer = projectileLayer;
        
        // Add collider
        CircleCollider2D collider = projectileObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
        
        // Add rigidbody for collision detection
        Rigidbody2D rb = projectileObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add projectile script
        BossProjectile projectile = projectileObj.AddComponent<BossProjectile>();
        projectile.Initialize(direction, _projectileSpeed, _projectileDamage, _bossController.PlayerLayer);
    }
    
    private Vector2 AngleToDirection(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}

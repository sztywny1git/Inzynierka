using System;
using UnityEngine;

/// <summary>
/// Controller for the Summoner Boss.
/// Phase 1: Alternates between melee attacks and summoning minions.
/// Phase 2: More aggressive, faster attacks, more frequent summons.
/// </summary>
public class SummonerBossController : BossController
{
    [Header("Summoner Boss Settings")]
    [SerializeField] private float phase1MoveSpeed = 2.5f;
    [SerializeField] private float phase2MoveSpeed = 4f;
    [SerializeField] private float meleeAttackRange = 3f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attack1Damage = 20f;
    [SerializeField] private float attack2Damage = 30f;
    [SerializeField] private float attack1Radius = 1.2f;
    [SerializeField] private float attack2Radius = 1.8f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("Summon Settings")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int phase1SummonCount = 2;
    [SerializeField] private int phase2SummonCount = 4;
    [SerializeField] private float summonCooldown = 8f;
    [SerializeField] private float summonRadius = 3f;
    [SerializeField] private Transform[] summonPoints;
    
    [Header("Ring Attack Settings")]
    [SerializeField] private int phase1RingProjectiles = 6;
    [SerializeField] private int phase2RingProjectiles = 10;
    [SerializeField] private float ringProjectileSpeed = 4f;
    [SerializeField] private float ringProjectileDamage = 15f;
    
    [Header("Attack Indicator")]
    [SerializeField] private bool showAttackIndicators = true;
    
    private SummonerBossAnimator _summonerAnimator;
    private BossAttackIndicator _attackIndicator;
    
    // Public accessors for states
    public float Phase1MoveSpeed => phase1MoveSpeed;
    public float Phase2MoveSpeed => phase2MoveSpeed;
    public float MeleeAttackRange => meleeAttackRange;
    public float Attack1Damage => attack1Damage;
    public float Attack2Damage => attack2Damage;
    public float Attack1Radius => attack1Radius;
    public float Attack2Radius => attack2Radius;
    public float AttackCooldown => attackCooldown;
    public GameObject MinionPrefab => minionPrefab;
    public int Phase1SummonCount => phase1SummonCount;
    public int Phase2SummonCount => phase2SummonCount;
    public float SummonCooldown => summonCooldown;
    public float SummonRadius => summonRadius;
    public Transform[] SummonPoints => summonPoints;
    public SummonerBossAnimator SummonerAnimator => _summonerAnimator;
    public BossAttackIndicator AttackIndicator => _attackIndicator;
    public bool ShowAttackIndicators => showAttackIndicators;
    
    // Ring attack accessors
    public int Phase1RingProjectiles => phase1RingProjectiles;
    public int Phase2RingProjectiles => phase2RingProjectiles;
    public float RingProjectileSpeed => ringProjectileSpeed;
    public float RingProjectileDamage => ringProjectileDamage;
    public LayerMask PlayerLayer => playerLayer;
    
    protected override void Awake()
    {
        base.Awake();
        _summonerAnimator = GetComponent<SummonerBossAnimator>();
        _attackIndicator = GetComponent<BossAttackIndicator>();
        
        // Add indicator if not present and enabled
        if (_attackIndicator == null && showAttackIndicators)
        {
            _attackIndicator = gameObject.AddComponent<BossAttackIndicator>();
        }
    }
    
    protected override void InitializePhaseStates()
    {
        Phase1State = new SummonerBossPhase1State(Context, this);
        Phase2State = new SummonerBossPhase2State(Context, this);
        
        if (debugLogging)
        {
            Debug.Log($"[SummonerBossController] Phase states initialized for {name}.", this);
        }
    }
    
    /// <summary>
    /// Spawn minions at the specified positions.
    /// </summary>
    public void SpawnMinions(int count)
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning("[SummonerBossController] Minion prefab not assigned!", this);
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }
        
        if (debugLogging)
        {
            Debug.Log($"[SummonerBossController] Spawned {count} minions.", this);
        }
    }
    
    private Vector3 GetSpawnPosition(int index)
    {
        // Use predefined spawn points if available
        if (summonPoints != null && summonPoints.Length > 0)
        {
            int pointIndex = index % summonPoints.Length;
            if (summonPoints[pointIndex] != null)
            {
                return summonPoints[pointIndex].position;
            }
        }
        
        // Otherwise spawn in a circle around the boss
        float angle = (360f / Mathf.Max(1, index + 1)) * index * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * summonRadius;
        return transform.position + offset;
    }
    
    /// <summary>
    /// Perform area damage for melee attacks.
    /// </summary>
    public void PerformAreaDamage(float damage, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer);
        
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var damageData = new DamageData(damage, false, gameObject, transform.position);
                damageable.TakeDamage(damageData);
            }
        }
        
        if (debugLogging && hits.Length > 0)
        {
            Debug.Log($"[SummonerBossController] Hit {hits.Length} targets for {damage} damage.", this);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Melee attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        
        // Attack 1 radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attack1Radius);
        
        // Attack 2 radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attack2Radius);
        
        // Summon radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }
}

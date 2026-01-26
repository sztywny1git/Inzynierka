using UnityEngine;

public class SummonerBossController : BossController
{
    [Header("Summoner Boss Settings")]
    [SerializeField] private float phase1MoveSpeed = 2.5f;
    [SerializeField] private float phase2MoveSpeed = 4f;
    
    [Header("Center Reference")]
    [SerializeField] private Transform centerPoint;

    [Header("Attack Settings")]
    [SerializeField] private float meleeAttackRange = 3f;
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
    [SerializeField] private float ringProjectileSpeed = 8f;
    [SerializeField] private float ringProjectileDamage = 15f;
    
    [Header("Attack Indicators")]
    [SerializeField] private bool showAttackIndicators = true;
    [SerializeField] private string indicatorLayerName = "Default";
    [SerializeField] private int indicatorOrder = 100;
    
    private SummonerBossAnimator _summonerAnimator;
    private BossAttackIndicator _attackIndicator;
    private RingAttackIndicator _ringAttackIndicator;
    
    public Vector3 CenterPosition => centerPoint != null ? centerPoint.position : transform.position;
    
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
    public RingAttackIndicator RingIndicator => _ringAttackIndicator;
    public bool ShowAttackIndicators => showAttackIndicators;
    
    public int Phase1RingProjectiles => phase1RingProjectiles;
    public int Phase2RingProjectiles => phase2RingProjectiles;
    public float RingProjectileSpeed => ringProjectileSpeed;
    public float RingProjectileDamage => ringProjectileDamage;
    public LayerMask PlayerLayer => playerLayer;
    
    protected override void Awake()
    {
        base.Awake();
        _summonerAnimator = GetComponent<SummonerBossAnimator>();
        
        if (showAttackIndicators)
        {
            _attackIndicator = GetComponent<BossAttackIndicator>();
            if (_attackIndicator == null)
            {
                _attackIndicator = gameObject.AddComponent<BossAttackIndicator>();
            }
            _attackIndicator.ConfigureSorting(indicatorLayerName, indicatorOrder);
            
            _ringAttackIndicator = GetComponent<RingAttackIndicator>();
            if (_ringAttackIndicator == null)
            {
                _ringAttackIndicator = gameObject.AddComponent<RingAttackIndicator>();
            }
            _ringAttackIndicator.ConfigureSorting(indicatorLayerName, indicatorOrder);
        }
    }
    
    protected override void InitializePhaseStates()
    {
        Phase1State = new SummonerBossPhase1State(Context, this);
        Phase2State = new SummonerBossPhase2State(Context, this);
    }
    
    public void SpawnMinions(int count)
    {
        if (minionPrefab == null) return;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }
    }
    
    private Vector3 GetSpawnPosition(int index)
    {
        if (summonPoints != null && summonPoints.Length > 0)
        {
            int pointIndex = index % summonPoints.Length;
            if (summonPoints[pointIndex] != null) return summonPoints[pointIndex].position;
        }
        
        float angle = (360f / Mathf.Max(1, index + 1)) * index * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * summonRadius;
        return CenterPosition + offset;
    }
    
    public void PerformAreaDamage(float damage, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(CenterPosition, radius, playerLayer);
        
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var damageData = new DamageData(damage, false, gameObject, CenterPosition);
                damageable.TakeDamage(damageData);
            }
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        Vector3 origin = CenterPosition;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, meleeAttackRange);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(origin, attack1Radius);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(origin, attack2Radius);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, summonRadius);
        
        if (centerPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, 0.3f);
            Gizmos.DrawLine(transform.position, origin);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(transform.position + Vector3.up * 2, "Warning", true);
        }
    }
}
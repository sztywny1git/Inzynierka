using UnityEngine;
using VContainer;

public class EnemyDeathHandler : BaseDeathHandler
{
    [Header("Loot Settings")]
    [SerializeField] private LootTableSO lootTable;
    
    [Header("Experience Settings")]
    [SerializeField] private int expValue = 10;

    [Header("Enemy Specifics")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 1.0f;

    private EnemyBrain _brain;
    private EnemyMovement _movement;
    private EnemyAnimator _enemyAnim;
    
    private GameplayEventBus _eventBus;

    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    protected override void Awake()
    {
        base.Awake();
        _brain = GetComponent<EnemyBrain>();
        _movement = GetComponent<EnemyMovement>();
        _enemyAnim = GetComponent<EnemyAnimator>();
    }

    protected override void HandleDeath()
    {
        if (_brain != null) _brain.enabled = false;
        if (_movement != null) _movement.Stop();
        
        if (_eventBus != null)
        {
            _eventBus.InvokeEnemyDied(transform.position, lootTable, expValue);
        }

        base.HandleDeath();
    }

    public override void OnDeathAnimationFinished()
    {
        base.OnDeathAnimationFinished();

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
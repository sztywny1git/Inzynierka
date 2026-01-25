using UnityEngine;
using VContainer;

[RequireComponent(typeof(BossController))]
public class BossRewardHandler : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private LootTableSO lootTable;
    [SerializeField] private int expValue = 150;

    private BossController _bossController;
    private GameplayEventBus _eventBus;

    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void Awake()
    {
        _bossController = GetComponent<BossController>();
    }

    private void OnEnable()
    {
        if (_bossController != null)
        {
            _bossController.OnBossDeath += HandleBossDeath;
        }
    }

    private void OnDisable()
    {
        if (_bossController != null)
        {
            _bossController.OnBossDeath -= HandleBossDeath;
        }
    }

    private void HandleBossDeath()
    {
        if (_eventBus != null)
        {
            _eventBus.InvokeEnemyDied(transform.position, lootTable, expValue);
        }
    }
}
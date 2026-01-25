using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;

public class LootSystem : IStartable, IDisposable
{
    private readonly GameplayEventBus _eventBus;
    private readonly Loot _lootPrefab;
    private Transform _parentContainer;

    [Inject]
    public LootSystem(GameplayEventBus eventBus, Loot lootPrefab, Transform parentContainer)
    {
        _eventBus = eventBus;
        _lootPrefab = lootPrefab;
        _parentContainer = parentContainer;
    }

    public void Start()
    {
        _eventBus.OnEnemyDied += HandleEnemyDeath;
    }

    public void Dispose()
    {
        _eventBus.OnEnemyDied -= HandleEnemyDeath;
    }

    public void SetLootContainer(Transform container)
    {
        _parentContainer = container;
    }

    private void HandleEnemyDeath(Vector3 position, LootTableSO lootTable, int expAmount)
    {
        if (lootTable == null) return;

        if (UnityEngine.Random.value > lootTable.dropChance) return;

        ItemSO selectedTemplate = lootTable.GetRandomItem();
        
        if (selectedTemplate != null)
        {
            int quantity = 1;

            if (selectedTemplate.isGold || selectedTemplate.stackSize > 1)
            {
                quantity = UnityEngine.Random.Range(selectedTemplate.minDropAmount, selectedTemplate.maxDropAmount + 1);
                SpawnLoot(selectedTemplate, quantity, position);
            }
            else
            {
                ItemSO itemInstance = selectedTemplate.CreateRandomInstance();
                SpawnLoot(itemInstance, 1, position);
            }
        }
    }

    private void SpawnLoot(ItemSO itemInstance, int quantity, Vector3 position)
    {
        position.z = -1f;

        Loot lootObj = UnityEngine.Object.Instantiate(_lootPrefab, position, Quaternion.identity, _parentContainer);
        lootObj.Initialize(itemInstance, quantity);
        
        ApplyDropForce(lootObj);
    }

    private void ApplyDropForce(Loot loot)
    {
        Rigidbody2D rb = loot.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDir * 5f, ForceMode2D.Impulse);
        }
    }
}
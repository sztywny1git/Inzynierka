using UnityEngine;
using System;

public class StatMediator : MonoBehaviour
{
    [SerializeField] private Character character;

    [Header("Stat Definitions Mapping")]
    public StatDefinition DamageDef;
    public StatDefinition ArmorDef;
    public StatDefinition MoveSpeedDef;
    public StatDefinition MaxHealthDef;
    public StatDefinition MaxResourceDef;
    public StatDefinition AttackSpeedDef;
    public StatDefinition CritChanceDef;
    public StatDefinition CritDamageDef;

    private void Awake()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }
    }

    private void Start()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.RegisterPlayerMediator(this);
        }

        if (AugmentManager.Instance != null)
        {
            AugmentManager.Instance.RegisterPlayerMediator(this);
        }

        if (StatsUI.Instance != null && character != null)
        {
            StatsUI.Instance.RegisterPlayerStats(character.Stats as CharacterStats);
        }

        if (InventoryManager.Instance != null && InventoryManager.Instance.useItem != null)
        {
            InventoryManager.Instance.useItem.RegisterPlayer(this);
        }
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.UnregisterPlayerMediator(this);

        if (AugmentManager.Instance != null)
            AugmentManager.Instance.UnregisterPlayerMediator(this);
            
        if (InventoryManager.Instance != null && InventoryManager.Instance.useItem != null)
            InventoryManager.Instance.useItem.UnregisterPlayer();
    }

    public void HandleEquipment(ItemSO item, bool isEquipping)
    {
        if (character == null || character.Stats == null) return;

        string source = $"Equipment_{item.name}";

        if (isEquipping)
        {
            ApplyStat(DamageDef, item.damage, source);
            ApplyStat(ArmorDef, item.armor, source);
            ApplyStat(MoveSpeedDef, item.speed, source);
            ApplyStat(MaxHealthDef, item.currentHearts, source);
            ApplyStat(MaxResourceDef, item.Resource, source);
            ApplyStat(AttackSpeedDef, item.fireRate, source);
            ApplyStat(CritChanceDef, item.CriticalChance, source);
            ApplyStat(CritDamageDef, item.CriticalDamage, source);
        }
        else
        {
            RemoveStatsFromSource(source);
        }
    }

    public void HandleConsumable(ItemSO item)
    {
        if (character == null) return;

        string source = $"Consumable_{item.name}_{Time.time}";

        if (item.currentHearts != 0 && character.Health != null)
        {
            character.Health.Heal(item.currentHearts);
        }

        if (item.Resource != 0) 
        {
             // Logika resource (np. mana)
        }

        if (character.Stats != null)
        {
            if (item.speed != 0) ApplyStat(MoveSpeedDef, item.speed, source, item.duration);
            if (item.armor != 0) ApplyStat(ArmorDef, item.armor, source, item.duration);
            if (item.damage != 0) ApplyStat(DamageDef, item.damage, source, item.duration);
            if (item.fireRate != 0) ApplyStat(AttackSpeedDef, item.fireRate, source, item.duration);
        }
    }

    public void HandleAugment(AugmentSO augment, int stacks)
    {
        if (character == null || character.Stats == null) return;

        string source = $"Augment_{augment.augmentName}";
        
        RemoveStatsFromSource(source);

        float totalValue = augment.value * stacks;

        switch (augment.augmentType)
        {
            case AugmentType.HealthBoost:
                ApplyStat(MaxHealthDef, totalValue, source);
                if (character.Health != null) character.Health.Heal(totalValue);
                break;
            case AugmentType.DamageBoost:
                ApplyStat(DamageDef, totalValue, source);
                break;
            case AugmentType.SpeedBoost:
                ApplyStat(MoveSpeedDef, totalValue, source);
                break;
            case AugmentType.CriticalChance:
                ApplyStat(CritChanceDef, totalValue, source);
                break;
            case AugmentType.AttackSpeedBoost:
                ApplyStat(AttackSpeedDef, totalValue, source);
                break;
            case AugmentType.ArmorBoost:
                ApplyStat(ArmorDef, totalValue, source);
                break;
            case AugmentType.ResourceBoost:
                ApplyStat(MaxResourceDef, totalValue, source);
                break;
        }
    }

    private void ApplyStat(StatDefinition def, float value, string source, float duration = -1f)
    {
        if (def != null && value != 0)
        {
            character.Stats.AddModifier(def, new StatModifier(value, ModifierType.Flat, source, duration));
        }
    }

    private void RemoveStatsFromSource(string source)
    {
        if (character == null || character.Stats == null) return;
        
        character.Stats.RemoveAllModifiersFromSource(DamageDef, source);
        character.Stats.RemoveAllModifiersFromSource(ArmorDef, source);
        character.Stats.RemoveAllModifiersFromSource(MoveSpeedDef, source);
        character.Stats.RemoveAllModifiersFromSource(MaxHealthDef, source);
        character.Stats.RemoveAllModifiersFromSource(MaxResourceDef, source);
        character.Stats.RemoveAllModifiersFromSource(AttackSpeedDef, source);
        character.Stats.RemoveAllModifiersFromSource(CritChanceDef, source);
        character.Stats.RemoveAllModifiersFromSource(CritDamageDef, source);
    }
}
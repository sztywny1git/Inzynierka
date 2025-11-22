using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    // --- Core stats ---
    public Stat Health = new Stat(100f);
    public Stat Armor = new Stat(0f);
    public Stat Defense = new Stat(0f);
    public Stat MoveSpeed = new Stat(1f);
    public Stat Damage = new Stat(10f);
    public Stat AttackSpeed = new Stat(1f);
    public Stat CriticalChance = new Stat(0.05f); // 5%
    public Stat CriticalDamage = new Stat(2f);    // x2
    public Stat Resource = new Stat(100f);

    // --- Secondary stats ---
    public SpecialStat ProjectileCount = new SpecialStat(1, 3);
    public SpecialStat ProjectileSize = new SpecialStat(1f, 5f);
    public SpecialStat Pierce = new SpecialStat(0, 5);
    public SpecialStat Ricochet = new SpecialStat(0, 5);
    public SpecialStat CooldownReduction = new SpecialStat(0, 1f);
    public SpecialStat DashCooldownReduction = new SpecialStat(0, 1f);
    public SpecialStat StatusChance = new SpecialStat(0, 1f);

    // --- Current values ---
    public float CurrentHealth { get; private set; }
    public float CurrentResource { get; private set; }

    // Events for UI updates
    public event Action<float, float> OnHealthChangedEvent;
    public event Action<float, float> OnResourceChangedEvent;

    private void Awake()
    {

        Health.BaseValue = 150f;    // test max HP
        CurrentHealth = 100f;
        CurrentResource = Resource.FinalValue;

        // Subscribe to stat changes
        Health.OnStatChanged += value => OnHealthChangedEvent?.Invoke(CurrentHealth, Health.FinalValue);
        Resource.OnStatChanged += value => OnResourceChangedEvent?.Invoke(CurrentResource, Resource.FinalValue);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Update all stat modifiers (buffs/debuffs)
        Health.UpdateModifiers(deltaTime);
        Armor.UpdateModifiers(deltaTime);
        Defense.UpdateModifiers(deltaTime);
        MoveSpeed.UpdateModifiers(deltaTime);
        Damage.UpdateModifiers(deltaTime);
        AttackSpeed.UpdateModifiers(deltaTime);
        CriticalChance.UpdateModifiers(deltaTime);
        CriticalDamage.UpdateModifiers(deltaTime);
        Resource.UpdateModifiers(deltaTime);
    }

    // Safely set current health
    public void SetCurrentHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, Health.FinalValue);
        OnHealthChangedEvent?.Invoke(CurrentHealth, Health.FinalValue);
    }

    // Safely set current resource
    public void SetCurrentResource(float value)
    {
        CurrentResource = Mathf.Clamp(value, 0f, Resource.FinalValue);
        OnResourceChangedEvent?.Invoke(CurrentResource, Resource.FinalValue);
    }
}

using UnityEngine;
using System;

[RequireComponent(typeof(IStatsProvider))]
public class Health : MonoBehaviour, IDamageable, ILiving, IHealable, IHealthProvider
{
    [SerializeField] private StatDefinition healthStatDef;
    private IStatsProvider _statsProvider;

    public bool isAlive => CurrentHealth > 0;
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action Death;

    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();
    }

    private void OnEnable()
    {
        if (_statsProvider != null)
        {
            _statsProvider.OnStatsReinitialized += ReinitializeHealth;
            ReinitializeHealth();
        }
    }

    private void OnDisable()
    {
        if (_statsProvider != null)
        {
            _statsProvider.OnStatsReinitialized -= ReinitializeHealth;

            var healthStat = _statsProvider.GetStat(healthStatDef);
            if (healthStat != null)
            {
                healthStat.OnStatChanged -= OnMaxHealthChanged;
            }
        }
    }

    private void ReinitializeHealth()
    {
        var oldStat = _statsProvider.GetStat(healthStatDef);
        if (oldStat != null) oldStat.OnStatChanged -= OnMaxHealthChanged;

        var newStat = _statsProvider.GetStat(healthStatDef);
        if (newStat != null)
        {
            newStat.OnStatChanged += OnMaxHealthChanged;
            OnMaxHealthChanged(newStat.FinalValue);
        }
    }

    private void OnMaxHealthChanged(float newMaxValue)
    {
        float oldMaxHealth = MaxHealth;
        float diff = newMaxValue - oldMaxHealth;

        MaxHealth = newMaxValue;

        if (diff > 0)
        {
            CurrentHealth += diff;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        }

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(DamageData damageData)
    {
        if (!isAlive) return;

        CurrentHealth = Mathf.Max(CurrentHealth - damageData.Amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (!isAlive)
        {
            Death?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (!isAlive || amount <= 0) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
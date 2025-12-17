using UnityEngine;
using System;
using VContainer;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private StatDefinition healthStatDef;
    
    private IStatsProvider _statsProvider;
    private Character _character; 
    private GameplayEventBus _eventBus;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    
    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();

        _character = GetComponent<Character>();
        
        if (_statsProvider is CharacterStats characterStats)
        {
            characterStats.OnStatsReinitialized += ReinitializeHealth;
        }
        
        ReinitializeHealth();
    }

    private void OnDestroy()
    {
        if (_statsProvider is CharacterStats characterStats)
        {
            characterStats.OnStatsReinitialized -= ReinitializeHealth;
        }

        var healthStat = _statsProvider?.GetStat(healthStatDef);
        if (healthStat != null)
        {
            healthStat.OnStatChanged -= OnMaxHealthChanged;
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
        float healthPercent = (MaxHealth > 0) ? CurrentHealth / MaxHealth : 1f;
        MaxHealth = newMaxValue;
        CurrentHealth = MaxHealth * healthPercent;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float baseDamage)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Max(CurrentHealth - baseDamage, 0);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
            
            if (_eventBus != null && _character != null)
            {
                _eventBus.PublishCharacterDied(_character);
            }
        }
    }
}
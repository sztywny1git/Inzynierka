using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(IStatsProvider))]
public class Health : MonoBehaviour, IDamageable, IHealable, IHealthProvider
{
    [SerializeField] private StatDefinition healthStatDef;
    
    [Header("Invulnerability Settings")]
    [SerializeField] private bool useInvulnerabilityFrames = false;
    [SerializeField] private float invulnerabilityDuration = 1.0f;

    private IStatsProvider _statsProvider;
    private bool _isInvulnerable;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public event Action<float, float> OnHealthChanged;
    public event Action<DamageData> OnDamageTaken;
    public event Action<bool> OnInvulnerabilityChanged;
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
            if (healthStat != null) healthStat.OnStatChanged -= OnMaxHealthChanged;
        }
        
        if (_isInvulnerable)
        {
            _isInvulnerable = false;
            OnInvulnerabilityChanged?.Invoke(false);
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
        float diff = newMaxValue - MaxHealth;
        MaxHealth = newMaxValue;

        if (diff > 0)
            CurrentHealth += diff;
        else
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(DamageData damageData)
    {
        if (!IsAlive) return;

        if (useInvulnerabilityFrames && _isInvulnerable) return;

        float damage = damageData.Amount;
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnDamageTaken?.Invoke(damageData);

        if (useInvulnerabilityFrames && IsAlive)
        {
            InvulnerabilityAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        if (!IsAlive)
        {
            Death?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private async UniTaskVoid InvulnerabilityAsync(System.Threading.CancellationToken token)
    {
        _isInvulnerable = true;
        OnInvulnerabilityChanged?.Invoke(true);

        await UniTask.Delay(TimeSpan.FromSeconds(invulnerabilityDuration), cancellationToken: token);

        _isInvulnerable = false;
        OnInvulnerabilityChanged?.Invoke(false);
    }
}
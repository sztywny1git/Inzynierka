using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(IStatsProvider))]
public class Health : MonoBehaviour, IDamageable, IHealable, IHealthProvider
{
    [SerializeField] private StatDefinition healthStatDef;
    [SerializeField] private StatDefinition armorStatDef;
    
    [Header("Invulnerability Settings")]
    [SerializeField] private bool useInvulnerabilityFrames = false;
    [SerializeField] private float invulnerabilityDuration = 1.0f;

    private IStatsProvider _statsProvider;
    private bool _isFrameInvulnerable;
    private bool _isExternalInvulnerable;
    
    private bool _isInitialized = false;

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
        
        if (_isFrameInvulnerable)
        {
            _isFrameInvulnerable = false;
            OnInvulnerabilityChanged?.Invoke(false);
        }
        
        _isExternalInvulnerable = false;
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
        MaxHealth = newMaxValue;

        if (!_isInitialized)
        {
            CurrentHealth = MaxHealth;
            _isInitialized = true;
        }
        else
        {
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetInvulnerable(bool state)
    {
        _isExternalInvulnerable = state;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (!IsAlive) return;
        if (_isExternalInvulnerable) return;
        if (useInvulnerabilityFrames && _isFrameInvulnerable) return;

        float rawDamage = damageData.Amount;
        float armor = 0f;

        if (_statsProvider != null && armorStatDef != null)
        {
            armor = _statsProvider.GetFinalStatValue(armorStatDef);
        }

        if (armor < 0) armor = 0;

        float damageMultiplier = 100f / (100f + armor);
        float calculatedDamage = rawDamage * damageMultiplier;
        float finalDamage = Mathf.Floor(calculatedDamage);

        CurrentHealth = Mathf.Max(CurrentHealth - finalDamage, 0);

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
        _isFrameInvulnerable = true;
        OnInvulnerabilityChanged?.Invoke(true);

        await UniTask.Delay(TimeSpan.FromSeconds(invulnerabilityDuration), cancellationToken: token);

        _isFrameInvulnerable = false;
        OnInvulnerabilityChanged?.Invoke(false);
    }
}
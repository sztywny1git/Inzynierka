using UnityEngine;
using System;

[RequireComponent(typeof(IStatsProvider))]
public class Resource : MonoBehaviour, IResourceProvider
{
    [Header("Stats Definitions")]
    [SerializeField] private StatDefinition _maxResourceStatDef;
    [SerializeField] private StatDefinition _regenStatDef;
    
    [Header("Settings")]
    [SerializeField] private bool _isPercentageRegen = false;

    private IStatsProvider _statsProvider;

    public float CurrentValue { get; private set; }
    public float MaxValue { get; private set; }

    public event Action<float, float> OnResourceChanged;

    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();
    }

    private void OnEnable()
    {
        if (_statsProvider != null)
        {
            _statsProvider.OnStatsReinitialized += ReinitializeResource;
            ReinitializeResource();
        }
    }

    private void OnDisable()
    {
        if (_statsProvider != null)
        {
            _statsProvider.OnStatsReinitialized -= ReinitializeResource;

            var maxStat = _statsProvider.GetStat(_maxResourceStatDef);
            if (maxStat != null)
            {
                maxStat.OnStatChanged -= OnMaxResourceChanged;
            }
        }
    }

    private void Update()
    {
        if (_regenStatDef != null && CurrentValue < MaxValue)
        {
            float rawRegenValue = _statsProvider.GetFinalStatValue(_regenStatDef);
            
            if (rawRegenValue > 0)
            {
                float regenPerSecond;

                if (_isPercentageRegen)
                {
                    regenPerSecond = Mathf.Floor(MaxValue * (rawRegenValue / 100f));
                }
                else
                {
                    regenPerSecond = rawRegenValue;
                }

                if (regenPerSecond > 0)
                {
                    Restore(regenPerSecond * Time.deltaTime);
                }
            }
        }
    }

    private void ReinitializeResource()
    {
        var oldStat = _statsProvider.GetStat(_maxResourceStatDef);
        if (oldStat != null) oldStat.OnStatChanged -= OnMaxResourceChanged;

        var newStat = _statsProvider.GetStat(_maxResourceStatDef);
        if (newStat != null)
        {
            newStat.OnStatChanged += OnMaxResourceChanged;
            OnMaxResourceChanged(newStat.FinalValue);
        }
        else
        {
            MaxValue = 100f;
            CurrentValue = 100f;
            OnResourceChanged?.Invoke(CurrentValue, MaxValue);
        }
    }

    private void OnMaxResourceChanged(float newMaxValue)
    {
        float oldMax = MaxValue;
        float diff = newMaxValue - oldMax;

        MaxValue = newMaxValue;

        if (diff > 0)
        {
            CurrentValue += diff;
        }
        else
        {
            CurrentValue = Mathf.Min(CurrentValue, MaxValue);
        }

        OnResourceChanged?.Invoke(CurrentValue, MaxValue);
    }

    public bool HasEnough(float amount)
    {
        return CurrentValue >= amount;
    }

    public void Consume(float amount)
    {
        CurrentValue = Mathf.Clamp(CurrentValue - amount, 0, MaxValue);
        OnResourceChanged?.Invoke(CurrentValue, MaxValue);
    }

    public void Restore(float amount)
    {
        CurrentValue = Mathf.Clamp(CurrentValue + amount, 0, MaxValue);
        OnResourceChanged?.Invoke(CurrentValue, MaxValue);
    }
}
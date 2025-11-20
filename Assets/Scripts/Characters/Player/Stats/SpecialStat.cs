using UnityEngine;
using System;

[Serializable]
public class SpecialStat
{
    public float BaseValue;
    public float MaxValue = float.MaxValue;
    public float Value { get; private set; }

    // Event triggered when stat changes
    public event Action<float> OnStatChanged;

    public SpecialStat(float baseValue, float maxValue = float.MaxValue)
    {
        BaseValue = baseValue;
        MaxValue = maxValue;
        Value = Mathf.Min(BaseValue, MaxValue);
    }

    // Increase stat, clamped to MaxValue
    public void Add(float amount)
    {
        Value = Mathf.Min(Value + amount, MaxValue);
        OnStatChanged?.Invoke(Value);
    }

    // Decrease stat, clamped to zero
    public void Remove(float amount)
    {
        Value = Mathf.Max(Value - amount, 0f);
        OnStatChanged?.Invoke(Value);
    }

    // Set stat to a specific value within bounds
    public void Set(float value)
    {
        Value = Mathf.Clamp(value, 0f, MaxValue);
        OnStatChanged?.Invoke(Value);
    }
}

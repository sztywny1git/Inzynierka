using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Stat
{
    public float BaseValue;
    private List<StatModifier> modifiers = new List<StatModifier>();

    // Event triggered when final stat value changes
    public event Action<float> OnStatChanged;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    // Add a new modifier
    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        Recalculate();
    }

    // Remove a specific modifier
    public void RemoveModifier(StatModifier mod)
    {
        modifiers.Remove(mod);
        Recalculate();
    }

    // Remove all modifiers from a specific source
    public void RemoveModifierBySource(string source)
    {
        modifiers.RemoveAll(m => m.Source == source);
        Recalculate();
    }

    // Calculate final value including additive and multiplicative modifiers
    public float FinalValue
    {
        get
        {
            float additiveSum = modifiers.Where(m => m.IsAdditive).Sum(m => m.Value);
            float multiplicativeSum = modifiers.Where(m => !m.IsAdditive).Sum(m => m.Value);
            return (BaseValue + additiveSum) * (1f + multiplicativeSum);
        }
    }

    // Update modifiers with duration and remove expired ones
    public void UpdateModifiers(float deltaTime)
    {
        bool removedAny = false;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            StatModifier mod = modifiers[i];
            if (mod.Duration >= 0f)
            {
                mod.Duration -= deltaTime;
                if (mod.Duration <= 0f)
                {
                    modifiers.RemoveAt(i);
                    removedAny = true;
                }
            }
        }
        if (removedAny)
            Recalculate();
    }

    // Recalculate and trigger change event
    private void Recalculate()
    {
        OnStatChanged?.Invoke(FinalValue);
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public class Stat
{
    public float BaseValue;
    
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();
    public event Action<float> OnStatChanged;

    private float _lastFinalValue;
    public float FinalValue => _lastFinalValue;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
        _lastFinalValue = baseValue;
    }

    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        Recalculate();
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (_modifiers.Remove(mod))
        {
            Recalculate();
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(string source)
    {
        int numRemoved = _modifiers.RemoveAll(mod => mod.Source == source);
        if (numRemoved > 0)
        {
            Recalculate();
            return true;
        }
        return false;
    }

    public void UpdateModifiers(float deltaTime)
    {
        bool wasChanged = _modifiers.RemoveAll(mod => mod.Duration > 0 && (mod.Duration -= deltaTime) <= 0) > 0;
        if (wasChanged)
        {
            Recalculate();
        }
    }

    private void Recalculate()
    {
        float newFinalValue = CalculateFinalValue();
        if (Math.Abs(_lastFinalValue - newFinalValue) > 0.001f)
        {
            _lastFinalValue = newFinalValue;
            OnStatChanged?.Invoke(_lastFinalValue);
        }
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float percentAddSum = 0;

        foreach (var mod in _modifiers)
        {
            if (mod.Type == ModifierType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == ModifierType.PercentAdd)
            {
                percentAddSum += mod.Value;
            }
        }

        finalValue *= (1f + percentAddSum);

        foreach (var mod in _modifiers)
        {
            if (mod.Type == ModifierType.PercentMult)
            {
                finalValue *= mod.Value;
            }
        }

        return finalValue;
    }
}
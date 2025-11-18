using System;

[Serializable]
public class StatModifier
{
    public float Value;
    public bool IsAdditive;    // true = additive, false = multiplicative
    public string Source;      // Source of the modifier (e.g., buff, debuff, item, class)
    public float Duration;     // >=0 = temporary, -1 = permanent

    public StatModifier(float value, bool isAdditive, string source, float duration = -1f)
    {
        Value = value;
        IsAdditive = isAdditive;
        Source = source;
        Duration = duration;
    }
}

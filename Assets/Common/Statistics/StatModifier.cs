using System;

[Serializable]
public class StatModifier
{
    public readonly float Value;
    public readonly ModifierType Type;
    public readonly string Source;
    public float Duration;

    public StatModifier(float value, ModifierType type, string source, float duration = -1f)
    {
        Value = value;
        Type = type;
        Source = source;
        Duration = duration;
    }
}
using UnityEngine;
using System;

[Serializable]
public struct StatModifierConfig
{
    public StatDefinition Stat;
    public ModifierType Type;
    public float Value;
}
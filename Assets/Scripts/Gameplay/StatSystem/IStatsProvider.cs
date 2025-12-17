using System;

public interface IStatsProvider
{
    Stat GetStat(StatDefinition definition);
    float GetFinalStatValue(StatDefinition definition);
    void AddModifier(StatDefinition definition, StatModifier modifier);
    void RemoveModifier(StatDefinition definition, StatModifier modifier);
    void RemoveAllModifiersFromSource(StatDefinition definition, string source);
    
}
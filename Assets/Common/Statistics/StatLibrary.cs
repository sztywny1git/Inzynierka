using UnityEngine;

[CreateAssetMenu(menuName = "Config/Stat System Config")]
public class StatSystemConfig : ScriptableObject
{
    [Header("Combat")]
    public StatDefinition DamageStat;
    public StatDefinition CritChanceStat;
    public StatDefinition CritMultiplierStat;
    public StatDefinition AttackSpeedStat;
}
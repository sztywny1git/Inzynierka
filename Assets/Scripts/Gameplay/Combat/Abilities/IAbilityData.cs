public class AbilitySnapshot
{
    public float BaseDamage { get; }
    public float CritChance { get; }
    public float CritMultiplier { get; }

    public AbilitySnapshot(float damage, float critChance, float critMult)
    {
        BaseDamage = damage;
        CritChance = critChance;
        CritMultiplier = critMult;
    }
}
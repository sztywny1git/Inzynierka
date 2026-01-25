using UnityEngine;

public class GameItem
{
    public ItemSO BaseData { get; private set; }
    
    public int CurrentDamage { get; private set; }
    public int CurrentArmor { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float CurrentCriticalChance { get; private set; }
    public float CurrentCriticalDamage { get; private set; }

    public GameItem(ItemSO source)
    {
        BaseData = source;
        InitializeStats(source);
    }

    private void InitializeStats(ItemSO source)
    {
        CurrentDamage = Mathf.RoundToInt(source.damage * GetRandomMultiplier());
        CurrentArmor = Mathf.RoundToInt(source.armor * GetRandomMultiplier());
        
        CurrentSpeed = source.speed * GetRandomMultiplier();
        CurrentCriticalChance = source.CriticalChance * GetRandomMultiplier();
        CurrentCriticalDamage = source.CriticalDamage * GetRandomMultiplier();
    }

    private float GetRandomMultiplier()
    {
        return Random.Range(0.5f, 2.0f);
    }
}
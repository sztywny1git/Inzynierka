public enum GameState
{
    None,
    MainMenu,
    Gameplay
}

public enum EnemyType
{
    Goblin,
    Skeleton
}

public enum ModifierType
{
    Flat,
    PercentAdd,
    PercentMult
}
public enum StatType
{
    Health,
    Armor,
    Defense,
    MoveSpeed,
    Damage,
    AttackSpeed,
    CriticalChance,
    CriticalDamage,
    Resource
}

public enum SpecialStatType
{
    ProjectileCount,
    ProjectileSize,
    Pierce,
    Ricochet,
    CooldownReduction,
    DashCooldownReduction,
    StatusChance
}
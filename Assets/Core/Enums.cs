public enum GameStateId
{
    Boot,
    MainMenu,
    Hub,
    Run,
    RunSummary
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

public enum RunOutcome
{
    Unknown, 
    Victory,
    Defeat
}

public enum AugmentType
{
    HealthBoost,
    DamageBoost,
    SpeedBoost,
    CriticalChance,
    AttackSpeedBoost,
    ArmorBoost,
    ResourceBoost
}

public enum CasterState
{
    Idle,
    PreCast,
    PostCast
}

public enum CharacterType 
{ 
    Player,
    Enemy,
    Boss 
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}


public enum ItemType
{
    Consumable,
    Collectible,
    Ring,
    Weapon,   
    Helmet,
    Chestplate,
    Legs,
    Boots      
}

public enum RoomType
{
    Undefined,
    Start,
    Standard,
    Puzzle,
    Boss
}

public enum TileType 
{ 
    Empty,
    Chest,
    Table, 
    Torch,
    Wall,
    Barrel,
    Vase,
    Pillar,
    Rock}

public enum SpawnMode
{
    FromDungeonGenerator,
    FromTilemap,
    FromManualPoints
}

public enum PropType 
{ 
    Empty, 
    Pillar, 
    Torch, 
    Crate, 
    Barrel, 
    Stone, 
    Vase, 
    Rug,   
    Fireplace   
}
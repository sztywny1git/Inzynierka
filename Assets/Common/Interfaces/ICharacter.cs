using UnityEngine;

public interface ICharacter
{
    IStatsProvider Stats { get; }
    Health Health { get; }
    AbilityCaster AbilityCaster { get; }
    
    CharacterType Type { get; }
    GameObject GameObject { get; }
}
using UnityEngine;
using VContainer;

public readonly struct AbilityContext
{
    public readonly GameObject Owner;
    public readonly Transform Origin;
    public readonly Vector2 Direction;
    public readonly IStatsProvider Stats;
    public readonly IObjectResolver DIContainer;
    public readonly Ability Ability;
    public readonly IResourceSystem ResourceSystem;

    public AbilityContext(
        GameObject owner,
        Transform origin,
        Vector2 direction,
        IStatsProvider stats,
        IObjectResolver diContainer,
        Ability ability,
        IResourceSystem resourceSystem)
    {
        Owner = owner;
        Origin = origin;
        Direction = direction;
        Stats = stats;
        DIContainer = diContainer;
        Ability = ability;
        ResourceSystem = resourceSystem;
    }
}
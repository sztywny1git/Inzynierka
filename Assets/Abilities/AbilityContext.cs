using UnityEngine;
using VContainer;

public readonly struct AbilityContext
{
    public readonly GameObject Instigator;
    public readonly IAbilitySpawner Spawner;
    public readonly IResourceProvider ResourceProvider;
    public readonly ICooldownProvider CooldownProvider;
    public readonly IStatsProvider StatsProvider;
    public readonly StatSystemConfig StatConfig;
    public readonly Transform Origin;
    public readonly Vector3 AimLocation;
    public readonly ActionIdentifier ActionId;

    public AbilityContext(
        GameObject instigator, 
        IAbilitySpawner spawner, 
        IResourceProvider resourceProvider, 
        ICooldownProvider cooldownProvider, 
        IStatsProvider statsProvider, 
        StatSystemConfig statConfig, 
        Transform origin, 
        Vector3 aimLocation, 
        ActionIdentifier actionId)
    {
        Instigator = instigator;
        Spawner = spawner;
        ResourceProvider = resourceProvider;
        CooldownProvider = cooldownProvider;
        StatsProvider = statsProvider;
        StatConfig = statConfig;
        Origin = origin;
        AimLocation = aimLocation;
        ActionId = actionId;
    }
}
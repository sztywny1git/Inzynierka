using UnityEngine;

public class AbilityContext
{
    public GameObject GameObject { get; }
    public IAbilitySpawner Spawner { get; }
    public IResourceProvider ResourceProvider { get; }
    public ICooldownProvider CooldownProvider { get; }
    public IStatsProvider StatsProvider { get; }
    public StatSystemConfig StatConfig { get; }
    
    public Transform Origin { get; }
    public Vector3 AimLocation { get; }
    public GameObject Instigator { get; }
    public ActionIdentifier ActionId { get; }

    public AbilityContext(
        GameObject gameObject,
        IAbilitySpawner spawner,
        IResourceProvider resourceProvider,
        ICooldownProvider cooldownProvider,
        IStatsProvider statsProvider,
        StatSystemConfig statConfig,
        Transform origin,
        Vector3 aimLocation,
        ActionIdentifier actionId)
    {
        GameObject = gameObject;
        Spawner = spawner;
        ResourceProvider = resourceProvider;
        CooldownProvider = cooldownProvider;
        StatsProvider = statsProvider;
        StatConfig = statConfig;
        Origin = origin;
        AimLocation = aimLocation;
        Instigator = gameObject;
        ActionId = actionId;
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;

public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<PlayerClassController>(Lifetime.Scoped).AsSelf().As<IDisposable>();
        builder.Register<RunProgressionManager>(Lifetime.Scoped).AsSelf().As<IDisposable>();
        builder.Register<PlayerSpawnManager>(Lifetime.Scoped);
        
        builder.Register<DungeonGenerator>(Lifetime.Scoped);
        builder.Register<LevelBuilder>(Lifetime.Scoped);

        builder.Register<PlayerFactory>(Lifetime.Scoped).As<IPlayerFactory>();
        builder.Register<EnemyFactory>(Lifetime.Scoped).As<IEnemyFactory>();
        builder.Register<AbilitySpawner>(Lifetime.Scoped).As<IAbilitySpawner>();
    }
}
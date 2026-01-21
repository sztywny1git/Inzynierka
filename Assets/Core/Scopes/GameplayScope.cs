using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;

public class GameplayScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(transform);

        builder.Register<PlayerClassController>(Lifetime.Singleton).AsSelf().As<IInitializable, IDisposable>();
        builder.Register<RunProgressionManager>(Lifetime.Singleton).AsSelf().As<IDisposable>();
        builder.Register<PlayerSpawnManager>(Lifetime.Singleton);
        
        builder.Register<AbilitySpawner>(Lifetime.Singleton).As<IAbilitySpawner>();
        builder.Register<PlayerFactory>(Lifetime.Singleton).As<IPlayerFactory>();
        builder.Register<EnemyFactory>(Lifetime.Singleton).As<IEnemyFactory>();
    }
}
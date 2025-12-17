using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CooldownSystem>(Lifetime.Scoped).As<ICooldownProvider, ITickable>();
        builder.Register<PlayerFactory>(Lifetime.Scoped).As<IPlayerFactory>();
        builder.Register<PlayerClassController>(Lifetime.Scoped).AsSelf().As<System.IDisposable>();
        
        builder.RegisterEntryPoint<PlayerSpawnManager>();
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Unity.Cinemachine;
using System;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private UIManager uiManager;

    protected override void Configure(IContainerBuilder builder)
    {
        // Components from Scene
        builder.RegisterComponent(uiManager);
        builder.RegisterComponentInHierarchy<CinemachineCamera>();

        // Core Systems & Managers
        builder.Register<SceneContextManager>(Lifetime.Singleton).As<ISceneContextManager>();
        builder.Register<GameSessionState>(Lifetime.Singleton);
        builder.Register<GameFlowManager>(Lifetime.Singleton).AsSelf().As<IInitializable, IDisposable>();

        // Global Services
        builder.Register<ResourceLoader>(Lifetime.Singleton).As<IResourceLoader>();
        builder.Register<CameraService>(Lifetime.Singleton).As<ICameraService>();
        builder.Register<InputService>(Lifetime.Singleton).As<IInputService, IInitializable, ITickable, System.IDisposable>();
        builder.Register<GameplayEventBus>(Lifetime.Singleton);
        builder.Register<UIEventBus>(Lifetime.Singleton);
        builder.Register<GameScopeService>(Lifetime.Singleton);
        
        
        // Entry Point
        builder.RegisterEntryPoint<GameBootstrapper>().As<IAsyncStartable>();
    }
}
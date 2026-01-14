using UnityEngine;
using VContainer;
using VContainer.Unity;
using Unity.Cinemachine;
using System;

public class GlobalScope : LifetimeScope
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameConstants gameConstants;    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(gameConstants);
        // Components from Scene
        builder.RegisterComponent(uiManager).AsSelf().As<IDisposable>();
        builder.RegisterComponentInHierarchy<CinemachineCamera>();

        // Core Systems & Managers
        builder.Register<GameFlowManager>(Lifetime.Singleton).AsSelf().As<IInitializable, IDisposable>();
        builder.Register<SceneContextManager>(Lifetime.Singleton).As<ISceneContextManager>();
        builder.Register<SessionData>(Lifetime.Singleton);
        builder.Register<GameScopeService>(Lifetime.Singleton);
        builder.Register<ResourceLoader>(Lifetime.Singleton).As<IResourceLoader>();

        // Game States
        builder.Register<MainMenuState>(Lifetime.Singleton);
        builder.Register<HubState>(Lifetime.Singleton);
        builder.Register<RunState>(Lifetime.Singleton);
        builder.Register<RunSummaryState>(Lifetime.Singleton);

        // Global Services
        builder.Register<TimeService>(Lifetime.Singleton);
        builder.Register<CameraService>(Lifetime.Singleton).As<ICameraService>();
        builder.Register<InputService>(Lifetime.Singleton).As<IInputService, IInitializable, ITickable, System.IDisposable>();
        builder.Register<SaveDataRepository>(Lifetime.Singleton);
        builder.Register<SaveGameManager>(Lifetime.Singleton);
        builder.Register<ProfileService>(Lifetime.Singleton).AsSelf().As<IInitializable, IDisposable>();
        

        // Event Buses
        builder.Register<GameplayEventBus>(Lifetime.Singleton);
        builder.Register<UIEventBus>(Lifetime.Singleton);
        
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Unity.Cinemachine;
using System;

public class GlobalScope : LifetimeScope
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private GameConstants gameConstants;    
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(gameConstants);

        builder.RegisterInstance(gameConstants.PlayerRegistry);
        builder.RegisterInstance(gameConstants.EnemyRegistry);
        
        builder.RegisterComponent(uiManager).AsSelf();
        builder.RegisterComponent(screenFader).AsSelf();
        builder.RegisterComponentInHierarchy<CinemachineCamera>();

        builder.Register<GameFlowManager>(Lifetime.Singleton).AsSelf().As<IInitializable, IDisposable>();
        builder.Register<SceneContextManager>(Lifetime.Singleton).As<ISceneContextManager>();
        builder.Register<SessionData>(Lifetime.Singleton);
        builder.Register<GameScopeService>(Lifetime.Singleton);

        builder.Register<MainMenuState>(Lifetime.Singleton);
        builder.Register<HubState>(Lifetime.Singleton);
        builder.Register<RunState>(Lifetime.Singleton);
        builder.Register<RunSummaryState>(Lifetime.Singleton);

        builder.Register<TimeService>(Lifetime.Singleton);
        builder.Register<CameraService>(Lifetime.Singleton).As<ICameraService>();
        builder.Register<InputService>(Lifetime.Singleton).As<IInputService, IInitializable, ITickable, IDisposable>();
        builder.Register<ScreenTransitionService>(Lifetime.Singleton);

        builder.Register<GameplayEventBus>(Lifetime.Singleton);
        builder.Register<UIEventBus>(Lifetime.Singleton);
    }
}
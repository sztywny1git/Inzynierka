using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var staticInitializer = GetComponentInChildren<StaticSceneInitializer>(true);
        if (staticInitializer != null)
        {
            builder.RegisterComponent(staticInitializer);
        }

        var proceduralInitializer = GetComponentInChildren<ProceduralSceneInitializer>(true);
        if (proceduralInitializer != null)
        {
            builder.RegisterComponent(proceduralInitializer);
        }
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelBuilder
{
    private readonly IResourceLoader _resourceLoader;
    private readonly IObjectResolver _container;

    public LevelBuilder(IResourceLoader resourceLoader, IObjectResolver container)
    {
        _resourceLoader = resourceLoader;
        _container = container;
    }
    //Mock
    public void Build(LevelData data)
    {

        var nextLevelPrefab = _resourceLoader.LoadPrefab("Prefabs/Level/NextStagePortal");
        if (nextLevelPrefab != null)
        {
            _container.Instantiate(nextLevelPrefab, data.NextLevelPortalPosition, Quaternion.identity);
        }

        var endRunPrefab = _resourceLoader.LoadPrefab("Prefabs/Level/RunWonPortal");
        if (endRunPrefab != null)
        {
            _container.Instantiate(endRunPrefab, data.EndRunPortalPosition, Quaternion.identity);
        }
    }
}
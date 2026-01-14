using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerFactory : IPlayerFactory
{
    private readonly IObjectResolver _container;
    private readonly IResourceLoader _resourceLoader;
    private readonly ICameraService _cameraService;
    private readonly GameplayEventBus _eventBus;

    public PlayerFactory(
        IObjectResolver container,
        IResourceLoader resourceLoader,
        ICameraService cameraService,
        GameplayEventBus eventBus)
    {
        _container = container;
        _resourceLoader = resourceLoader;
        _cameraService = cameraService;
        _eventBus = eventBus;
    }

    public GameObject CreatePlayer(CharacterDefinition classDef, Vector3 position)
    {
        string prefabPath = $"Prefabs/Player/{classDef.name}";
        var playerPrefab = _resourceLoader.LoadPrefab(prefabPath);

        if (playerPrefab == null)
        {
            Debug.LogError($"Failed to load player prefab at path: {prefabPath}. Make sure the prefab variant name matches the CharacterDefinition asset name.");
            return null;
        }

        var playerInstance = _container.Instantiate(playerPrefab, position, Quaternion.identity);
        var playerCharacter = playerInstance.GetComponent<Character>();

        if (playerCharacter != null)
        {
            _eventBus.InvokePlayerSpawned(playerCharacter);
        }

        if (playerInstance != null)
        {
            _cameraService.SetFollowTarget(playerInstance.transform);
        }
        
        return playerInstance;
    }
}
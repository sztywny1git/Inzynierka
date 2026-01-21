using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerFactory : IPlayerFactory
{
    private readonly IObjectResolver _container;
    private readonly PlayerRegistry _playerRegistry;
    private readonly ICameraService _cameraService;
    private readonly GameplayEventBus _eventBus;
    private readonly Transform _gameplayRoot;

    public PlayerFactory(
        IObjectResolver container,
        PlayerRegistry playerRegistry,
        ICameraService cameraService,
        GameplayEventBus eventBus,
        Transform gameplayRoot)
    {
        _container = container;
        _playerRegistry = playerRegistry;
        _cameraService = cameraService;
        _eventBus = eventBus;
        _gameplayRoot = gameplayRoot;
    }

    public GameObject CreatePlayer(CharacterDefinition classDef, Vector3 position)
    {
        var playerPrefab = _playerRegistry.GetPrefab(classDef);

        if (playerPrefab == null) return null;

        var playerInstance = _container.Instantiate(playerPrefab, position, Quaternion.identity, _gameplayRoot);
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
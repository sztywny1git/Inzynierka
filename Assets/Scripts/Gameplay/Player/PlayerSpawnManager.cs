using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerSpawnManager
{
    private readonly IPlayerFactory _playerFactory;
    private readonly SessionData _sessionData;
    private readonly LifetimeScope _currentScope;
    
    private Character _activePlayerInstance;

    public PlayerSpawnManager(
        IPlayerFactory playerFactory, 
        SessionData sessionData, 
        LifetimeScope currentScope)
    {
        _playerFactory = playerFactory;
        _sessionData = sessionData;
        _currentScope = currentScope;
    }

    public void SpawnPlayer(Transform spawnPoint)
    {
        if (_activePlayerInstance != null)
        {
            _activePlayerInstance.transform.position = spawnPoint.position;
            _activePlayerInstance.GetComponent<PlayerMovement>()?.StopMovement();
        }
        else
        {
            var playerClass = _sessionData.CurrentPlayerClass;
            if (playerClass != null)
            {
                var playerObj = _playerFactory.CreatePlayer(playerClass, spawnPoint.position);
                if (playerObj != null)
                {
                    _activePlayerInstance = playerObj.GetComponent<Character>();
                }
            }
        }
    }
}
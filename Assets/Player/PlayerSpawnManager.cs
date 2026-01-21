using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;

public class PlayerSpawnManager : IDisposable
{
    private readonly IPlayerFactory _playerFactory;
    private readonly SessionData _sessionData;
   
    private Character _activePlayerInstance;

    public PlayerSpawnManager(
        IPlayerFactory playerFactory,
        SessionData sessionData)
    {
        _playerFactory = playerFactory;
        _sessionData = sessionData;
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

    public void Dispose()
    {
        if (_activePlayerInstance != null)
        {
            UnityEngine.Object.Destroy(_activePlayerInstance.gameObject);
            _activePlayerInstance = null;
        }
    }
}
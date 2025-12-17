using UnityEngine;
using System;
using VContainer;

public class PlayerSpawnManager : IDisposable
{
    private readonly IPlayerFactory _playerFactory;
    private readonly GameSessionState _sessionState;
    private readonly GameplayEventBus _eventBus;
    private Character _activePlayerInstance;

    public PlayerSpawnManager(
        IPlayerFactory playerFactory, 
        GameSessionState sessionState, 
        GameplayEventBus eventBus)
    {
        _playerFactory = playerFactory;
        _sessionState = sessionState;
        _eventBus = eventBus;

        _eventBus.OnLevelReady += HandleLevelReady;
    }

    public void Dispose()
    {
        _eventBus.OnLevelReady -= HandleLevelReady;
    }

    private void HandleLevelReady(Transform spawnPoint)
    {
        if (_activePlayerInstance != null)
        {
            _activePlayerInstance.transform.position = spawnPoint.position;
            
            var movement = _activePlayerInstance.GetComponent<PlayerMovement>();
            if(movement != null) movement.StopMovement();
        }
        else
        {
            var playerClass = _sessionState.CurrentPlayerClass;
            if (playerClass != null)
            {
                var playerObj = _playerFactory.CreatePlayer(playerClass, spawnPoint.position);
                _activePlayerInstance = playerObj.GetComponent<Character>();
                
            }
        }
    }
}
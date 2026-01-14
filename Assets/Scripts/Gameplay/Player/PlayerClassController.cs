using UnityEngine;
using System;

public class PlayerClassController : IDisposable
{
    private readonly SessionData _sessionData;
    private readonly GameplayEventBus _eventBus;
    private readonly IPlayerFactory _playerFactory;
    
    private Character _activeCharacter;

    public PlayerClassController(
        SessionData  sessionData, 
        GameplayEventBus eventBus,
        IPlayerFactory playerFactory)
    {
        _sessionData = sessionData;
        _eventBus = eventBus;
        _playerFactory = playerFactory;
        
        _eventBus.PlayerSpawned += HandlePlayerSpawned;
        _eventBus.ClassSelected += HandleClassSelected;
    }

    public void Dispose()
    {
        _eventBus.PlayerSpawned -= HandlePlayerSpawned;
        _eventBus.ClassSelected -= HandleClassSelected;
    }

    private void HandlePlayerSpawned(Character character)
    {
        _activeCharacter = character;
    }

    private void HandleClassSelected(CharacterDefinition classDef)
    {
        if (classDef == null) return;
        if (_sessionData.CurrentPlayerClass == classDef) return;

        _sessionData.CurrentPlayerClass = classDef;

        if (_activeCharacter != null)
        {
            SwapPlayerObject(classDef);
        }
    }

    private void SwapPlayerObject(CharacterDefinition classDef)
    {
        Vector3 position = _activeCharacter.transform.position;

        UnityEngine.Object.Destroy(_activeCharacter.gameObject);

        _playerFactory.CreatePlayer(classDef, position);
    }
}
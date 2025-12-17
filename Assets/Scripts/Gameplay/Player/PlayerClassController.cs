using UnityEngine;
using System;

public class PlayerClassController : IDisposable
{
    private readonly GameSessionState _sessionState;
    private readonly GameplayEventBus _eventBus;
    
    private Character _activeCharacter;

    public PlayerClassController(GameSessionState sessionState, GameplayEventBus eventBus)
    {
        _sessionState = sessionState;
        _eventBus = eventBus;

        _eventBus.OnPlayerSpawned += OnPlayerSpawned;
        _eventBus.OnClassSelected += OnClassSelected;
    }

    public void Dispose()
    {
        _eventBus.OnPlayerSpawned -= OnPlayerSpawned;
        _eventBus.OnClassSelected -= OnClassSelected;
    }

    private void OnPlayerSpawned(Character character)
    {
        _activeCharacter = character;
        
        if (_sessionState.CurrentPlayerClass != null)
        {
            _activeCharacter.ApplyCharacterDefinition(_sessionState.CurrentPlayerClass);
        }
    }

    private void OnClassSelected(CharacterDefinition classDef)
    {
        if (classDef == null) return;

        _sessionState.CurrentPlayerClass = classDef;

        if (_activeCharacter != null)
        {
            _activeCharacter.ApplyCharacterDefinition(classDef);
        }
    }
}
using UnityEngine;
using VContainer;
using System;
using System.Collections.Generic;

public class HubNPCManager : MonoBehaviour
{
    [Serializable]
    public struct ClassNpcBinding
    {
        public CharacterDefinition Definition;
        public GameObject NpcObject;
    }

    [SerializeField] private List<ClassNpcBinding> _npcBindings;
    
    private GameplayEventBus _gameplayEventBus;
    private SessionData _sessionData;

    [Inject]
    public void Construct(GameplayEventBus gameplayEventBus, SessionData sessionData)
    {
        _gameplayEventBus = gameplayEventBus;
        _sessionData = sessionData;
    }

    private void Start()
    {
        if (_gameplayEventBus != null)
        {
            _gameplayEventBus.ClassSelected += UpdateNpcVisibility;
        }

        if (_sessionData != null && _sessionData.CurrentPlayerClass != null)
        {
            UpdateNpcVisibility(_sessionData.CurrentPlayerClass);
        }
    }

    private void OnDestroy()
    {
        if (_gameplayEventBus != null)
        {
            _gameplayEventBus.ClassSelected -= UpdateNpcVisibility;
        }
    }

    private void UpdateNpcVisibility(CharacterDefinition selectedClass)
    {
        foreach (var binding in _npcBindings)
        {
            if (binding.NpcObject == null) continue;

            bool shouldBeVisible = binding.Definition != selectedClass;
            binding.NpcObject.SetActive(shouldBeVisible);
        }
    }
}
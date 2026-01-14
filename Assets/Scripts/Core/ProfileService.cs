using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public class ProfileService : IInitializable, IDisposable
{
    private readonly SaveGameManager _saveManager;
    private readonly GameplayEventBus _gameplayEvents;
    private readonly UIEventBus _uiEvents;
    private readonly SessionData _sessionData;
    private readonly GameConstants _gameConstants;
    private readonly IResourceLoader _resourceLoader;

    public ProfileService(
        SaveGameManager saveManager, GameplayEventBus gameplayEvents, UIEventBus uiEvents,
        SessionData sessionData, GameConstants gameConstants, IResourceLoader resourceLoader)
    {
        _saveManager = saveManager;
        _gameplayEvents = gameplayEvents;
        _uiEvents = uiEvents;
        _sessionData = sessionData;
        _gameConstants = gameConstants;
        _resourceLoader = resourceLoader;

    }
    
    public void Initialize() 
    {
            _uiEvents.ProfileSelected += HandleProfileSelected;
    }
    public void Dispose()
    {
        _uiEvents.ProfileSelected -= HandleProfileSelected;
    }

    private void HandleProfileSelected(int slotId)
    {
        if (_saveManager.HasMetaSave(slotId))
        {
            LoadGame(slotId);
        }
        else
        {
            NewGame(slotId);
        }
    }    
    private void NewGame(int slotId)
    {
        _saveManager.SetActiveProfile(slotId);
        _sessionData.CurrentPlayerClass = _gameConstants.DefaultPlayerClass;
        _gameplayEvents.RequestGoToHub();
    }

    private void LoadGame(int slotId)
    {
        _saveManager.SetActiveProfile(slotId);
        var metaData = _saveManager.LoadMetaProgress();
        
        if (metaData != null && !string.IsNullOrEmpty(metaData.LastUsedCharacterClassID))
        {
            _sessionData.CurrentPlayerClass = _resourceLoader.Load<CharacterDefinition>(metaData.LastUsedCharacterClassID);
        }
        
        if (_sessionData.CurrentPlayerClass == null)
        {
            _sessionData.CurrentPlayerClass = _gameConstants.DefaultPlayerClass;
        }

        if (_saveManager.HasRunSave(slotId))
        {
            _gameplayEvents.RequestBeginRun();
        }
        else
        {
            _gameplayEvents.RequestGoToHub();
        }
    }
    

}
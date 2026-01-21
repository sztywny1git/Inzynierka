using System;
using UnityEngine;

public class RunProgressionManager : IDisposable
{
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionState;
    private readonly GameConstants _gameConstants;

    public RunProgressionManager(
        GameplayEventBus gameplayEvents, 
        SessionData sessionState,
        GameConstants gameConstants)
    {
        _gameplayEvents = gameplayEvents;
        _sessionState = sessionState;
        _gameConstants = gameConstants;
        
        _gameplayEvents.AdvanceToNextLevelRequested += AdvanceToNextLevel;
    }

    public void Dispose()
    {
        _gameplayEvents.AdvanceToNextLevelRequested -= AdvanceToNextLevel;
    }

    public void StartRun()
    {
        _sessionState.CurrentLevelIndex = -1;
        AdvanceToNextLevel();
    }

    private void AdvanceToNextLevel()
    {
        _sessionState.CurrentLevelIndex++;

        if (_sessionState.CurrentLevelIndex < _gameConstants.MaxLevels)
        {
            _gameplayEvents.RequestLoadLevel();
        }
        else
        {
            _gameplayEvents.InvokeRunWon();
        }
    }
}
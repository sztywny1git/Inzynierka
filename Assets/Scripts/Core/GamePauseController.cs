using System;
using VContainer;

public class GamePauseController : IDisposable
{
    private readonly IInputService _inputService;
    private readonly UIEventBus _uiEvents;

    public GamePauseController(IInputService inputService, UIEventBus uiEvents)
    {
        _inputService = inputService;
        _uiEvents = uiEvents;
        _inputService.OnPausePressed += HandlePauseInput;
    }

    public void Dispose()
    {
        _inputService.OnPausePressed -= HandlePauseInput;
    }

    private void HandlePauseInput()
    {
        _uiEvents.RequestTogglePauseMenu();
    }
}
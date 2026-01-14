using UnityEngine;
using VContainer;

public class PauseMenuPanel : MonoBehaviour
{
    private TimeService _timeService;

    [Inject]
    public void Construct(TimeService timeService)
    {
        _timeService = timeService;
    }

    private void OnEnable()
    {
        if (_timeService != null) 
            _timeService.RequestPause(this);
    }

    private void OnDisable()
    {
        if (_timeService != null) 
            _timeService.ReleasePause(this);
    }
}
using UnityEngine;

public class TimeService
{
    private int _pauseRequests = 0;

    public bool IsPaused => _pauseRequests > 0;

    public void RequestPause(object requester)
    {
        _pauseRequests++;
        if (_pauseRequests == 1)
        {
            Time.timeScale = 0f;
            Debug.Log($"Game Paused by: {requester.GetType().Name}");
        }
    }

    public void ReleasePause(object requester)
    {
        if (_pauseRequests > 0)
        {
            _pauseRequests--;
            if (_pauseRequests == 0)
            {
                Time.timeScale = 1f;
                Debug.Log($"Game Resumed by: {requester.GetType().Name}");
            }
        }
    }
}
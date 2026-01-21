using System;
using Cysharp.Threading.Tasks;

public class ScreenTransitionService
{
    private readonly ScreenFader _screenFader;

    public ScreenTransitionService(ScreenFader screenFader)
    {
        _screenFader = screenFader;
    }

    public async UniTask PerformTransition(Func<UniTask> operation)
    {
        await _screenFader.FadeInAsync();

        try
        {
            await operation.Invoke();
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
        finally
        {
            await _screenFader.FadeOutAsync();
        }
    }
}
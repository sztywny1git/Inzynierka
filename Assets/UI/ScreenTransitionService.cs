using System;
using Cysharp.Threading.Tasks;

public class ScreenTransitionService
{
    private readonly ScreenFader _screenFader;

    public ScreenTransitionService(ScreenFader screenFader)
    {
        _screenFader = screenFader;
    }

    public async UniTask PerformTransition(Func<UniTask> operation, bool autoFadeOut = true)
    {
        await _screenFader.FadeInAsync();

        try
        {
            await operation.Invoke();
            await UniTask.WaitForEndOfFrame();
        }
        finally
        {
            if (autoFadeOut)
            {
                await FadeOutAsync();
            }
        }
    }

    public async UniTask FadeInAsync()
    {
        await _screenFader.FadeInAsync();
    }

    public async UniTask FadeOutAsync()
    {
        await _screenFader.FadeOutAsync();
    }
}
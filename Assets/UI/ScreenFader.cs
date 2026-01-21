using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.5f;
    
    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public async UniTask FadeInAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        float timer = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (timer < _fadeDuration)
        {
            if (token.IsCancellationRequested) return;

            timer += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / _fadeDuration);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        _canvasGroup.alpha = 1f;
    }

    public async UniTask FadeOutAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        float timer = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (timer < _fadeDuration)
        {
            if (token.IsCancellationRequested) return;

            timer += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / _fadeDuration);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
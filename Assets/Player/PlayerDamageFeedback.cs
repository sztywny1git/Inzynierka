using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerDamageFeedback : BaseDamageFeedback
{
    [Header("Visual Settings")]
    [SerializeField] private float blinkInterval = 0.1f;
    
    [Header("Physics Settings")]
    [SerializeField] private int invincibleLayerIndex; 
    
    private int _defaultLayer;
    private IHealthProvider _health;
    private SpriteRenderer _spriteRenderer;
    private CancellationTokenSource _blinkCts;

    protected override void Awake()
    {
        base.Awake();
        _health = GetComponent<IHealthProvider>();
        _defaultLayer = gameObject.layer;
        
        _spriteRenderer = Renderer as SpriteRenderer;
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (_health != null)
            _health.OnInvulnerabilityChanged += HandleInvulnerabilityChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_health != null)
            _health.OnInvulnerabilityChanged -= HandleInvulnerabilityChanged;

        ResetState();
    }

    private void HandleInvulnerabilityChanged(bool isInvulnerable)
    {
        if (isInvulnerable)
            StartInvulnerabilityEffect();
        else
            ResetState();
    }

    private void StartInvulnerabilityEffect()
    {
        gameObject.layer = invincibleLayerIndex;

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
        _blinkCts = new CancellationTokenSource();

        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_blinkCts.Token, this.GetCancellationTokenOnDestroy()).Token;
        BlinkAsync(linkedToken).Forget();
    }

    private void ResetState()
    {
        gameObject.layer = _defaultLayer;

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
        _blinkCts = null;

        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }
        else if (Renderer != null) 
        {
            Renderer.enabled = true;
        }
    }

    private async UniTaskVoid BlinkAsync(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(flashDuration), cancellationToken: token);

        bool isTransparent = false;

        while (!token.IsCancellationRequested)
        {
            isTransparent = !isTransparent;

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = isTransparent ? 0f : 1f;
                _spriteRenderer.color = c;
            }
            else if (Renderer != null)
            {
                Renderer.enabled = !isTransparent;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(blinkInterval), cancellationToken: token);
        }
    }
}
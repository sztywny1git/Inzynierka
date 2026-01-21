using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(IHealthProvider))]
public abstract class BaseDamageFeedback : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] protected Color flashColor = Color.white;
    [SerializeField] protected float flashDuration = 0.1f;
    
    [SerializeField] protected string colorPropertyName = "_FlashColor";
    [SerializeField] protected string amountPropertyName = "_FlashAmount";

    protected IHealthProvider HealthProvider;
    protected Renderer Renderer;
    
    private int _colorID;
    private int _amountID;
    private Material _materialInstance;
    private CancellationTokenSource _flashCts;

    protected virtual void Awake()
    {
        HealthProvider = GetComponent<IHealthProvider>();
        Renderer = GetComponentInChildren<Renderer>();
           
        _colorID = Shader.PropertyToID(colorPropertyName);
        _amountID = Shader.PropertyToID(amountPropertyName);
    }

    protected virtual void OnEnable()
    {
        if (HealthProvider != null)
            HealthProvider.OnDamageTaken += OnDamageReceived;
    }

    protected virtual void OnDisable()
    {
        if (HealthProvider != null)
            HealthProvider.OnDamageTaken -= OnDamageReceived;
            
        _flashCts?.Cancel();
        _flashCts?.Dispose();
        _flashCts = null;

        if (_materialInstance != null)
        {
            _materialInstance.SetFloat(_amountID, 0f);
        }
    }

    protected virtual void OnDamageReceived(DamageData data)
    {
        _flashCts?.Cancel();
        _flashCts?.Dispose();
        _flashCts = new CancellationTokenSource();

        FlashAsync(_flashCts.Token).Forget();
    }

    protected async UniTaskVoid FlashAsync(CancellationToken token)
    {
        if (Renderer == null) return;

        if (_materialInstance == null)
             _materialInstance = Renderer.material;

        _materialInstance.SetColor(_colorID, flashColor);
        _materialInstance.SetFloat(_amountID, 1f);

        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(flashDuration), cancellationToken: token).SuppressCancellationThrow();

        if (!canceled && _materialInstance != null)
        {
            _materialInstance.SetFloat(_amountID, 0f);
        }
    }
}
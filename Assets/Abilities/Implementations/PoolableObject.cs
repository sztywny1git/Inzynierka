using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class PoolableObject : MonoBehaviour
{
    public event Action<PoolableObject> ReturnRequested;

    private CancellationTokenSource _lifetimeCts;
    private bool _isActive;

    public virtual void OnSpawn()
    {
        _isActive = true;
    }

    public virtual void OnDespawn()
    {
        CancelLifetimeTimer();
        _isActive = false;
        ReturnRequested = null; 
    }

    protected void SetLifetime(float lifetime)
    {
        CancelLifetimeTimer();
        _lifetimeCts = new CancellationTokenSource();

        var token = CancellationTokenSource.CreateLinkedTokenSource(
            _lifetimeCts.Token, 
            this.GetCancellationTokenOnDestroy()
        ).Token;

        LifetimeRoutine(lifetime, token).Forget();
    }

    private async UniTaskVoid LifetimeRoutine(float lifetime, CancellationToken token)
    {
        bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(lifetime), cancellationToken: token)
                                     .SuppressCancellationThrow();

        if (!canceled && _isActive)
        {
            ReturnToPool();
        }
    }

    private void CancelLifetimeTimer()
    {
        if (_lifetimeCts != null)
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }
    }
    protected virtual void Update() { }

    public void ReturnToPool()
    {
        if (!_isActive) return;
        ReturnRequested?.Invoke(this);
    }
}
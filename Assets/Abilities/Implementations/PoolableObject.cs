using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
    private IPoolReturner _returner;
    private float _lifetimeTimer;
    private bool _hasActiveLifetime;
    private bool _isPooled;

    public void InitializePool(IPoolReturner returner)
    {
        _returner = returner;
    }

    public virtual void OnSpawn()
    {
        _isPooled = true;
        _hasActiveLifetime = false;
    }

    public virtual void OnDespawn()
    {
        _hasActiveLifetime = false;
        _isPooled = false;
    }

    protected void SetLifetime(float lifetime)
    {
        _lifetimeTimer = lifetime;
        _hasActiveLifetime = true;
    }

    protected virtual void Update()
    {
        if (_hasActiveLifetime)
        {
            _lifetimeTimer -= Time.deltaTime;
            if (_lifetimeTimer <= 0)
            {
                ReturnToPool();
            }
        }
    }

    public void ReturnToPool()
    {
        if (!_isPooled) return;
        _hasActiveLifetime = false;
        _returner?.Return(this);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Action<GameObject> _despawnCallback;
    private GameObject _instigator;
    private IMovementStrategy _movementStrategy;
    private DamageData _damageData;
    private int _remainingPierce;
    private float _expireTime;
    private bool _isActive;
    
    private HashSet<GameObject> _hitHistory = new HashSet<GameObject>();

    public void Initialize(Action<GameObject> despawnCallback, GameObject instigator, IMovementStrategy strategy, DamageData damageData, int pierceCount, float lifetime)
    {
        _despawnCallback = despawnCallback;
        _instigator = instigator;
        _movementStrategy = strategy;
        _damageData = damageData;
        _remainingPierce = pierceCount;
        _expireTime = Time.time + lifetime;
        _isActive = true;
        _hitHistory.Clear();

        if (_movementStrategy != null)
        {
            _movementStrategy.Initialize(transform);
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        if (Time.time >= _expireTime)
        {
            Despawn();
            return;
        }

        if (_movementStrategy != null)
        {
            _movementStrategy.Update(Time.deltaTime);
            
            if (_movementStrategy.IsDone)
            {
                Despawn();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive) return;
        if (other.gameObject == _instigator) return;
        if (_hitHistory.Contains(other.gameObject)) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damageData);
            _hitHistory.Add(other.gameObject);

            if (_remainingPierce != -1)
            {
                if (_remainingPierce > 0)
                {
                    _remainingPierce--;
                }
                else
                {
                    Despawn();
                }
            }
        }
        else
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        _isActive = false;
        _despawnCallback?.Invoke(gameObject);
    }
}
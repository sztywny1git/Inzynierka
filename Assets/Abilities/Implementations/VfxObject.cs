using UnityEngine;

public class VfxObject : PoolableObject
{
    [SerializeField] private float _lifetime = 1.0f;
    
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        if (_particleSystem != null)
        {
            _particleSystem.Clear();
            _particleSystem.Play();
        }
        
        SetLifetime(_lifetime);
    }

    public override void OnDespawn()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        base.OnDespawn();
    }
}
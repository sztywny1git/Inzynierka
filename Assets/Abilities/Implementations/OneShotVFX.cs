using UnityEngine;
using System.Linq;

public class VfxObject : PoolableObject
{
    private Animator _animator;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        float duration = 0f;

        if (_particleSystem != null)
        {
            _particleSystem.Clear();
            _particleSystem.Play();
            duration = Mathf.Max(duration, _particleSystem.main.duration);
        }

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
            
            var clips = _animator.runtimeAnimatorController ? _animator.runtimeAnimatorController.animationClips : null;
            if (clips != null && clips.Length > 0)
            {
                duration = Mathf.Max(duration, clips[0].length);
            }
        }

        if (duration <= 0) duration = 1f;
        
        SetLifetime(duration);
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
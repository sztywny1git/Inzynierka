using UnityEngine;

public class VfxObject : PoolableObject
{
    [SerializeField] private float _lifetime = 1.0f;
    [SerializeField] private string _animationStateName = "Play";
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override void OnSpawn()
    {
        gameObject.SetActive(true);

        base.OnSpawn();

        if (_animator != null)
        {
            _animator.Play(_animationStateName, 0, 0f);
        }
        
        SetLifetime(_lifetime);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
    }
}
using UnityEngine;

public class EnemyTargetProvider : MonoBehaviour
{
    [SerializeField] private float reacquireInterval = 0.5f;

    private float _nextReacquireTime;
    private Transform _targetTransform;
    private PlayerController _targetController;

    public Transform Target
    {
        get
        {
            if (IsTargetInvalid())
            {
                if (Time.time >= _nextReacquireTime)
                {
                    Reacquire();
                }
            }
            
            return IsTargetValid() ? _targetTransform : null;
        }
    }

    private void Awake()
    {
        _nextReacquireTime = 0f;
        Reacquire();
    }

    private bool IsTargetValid()
    {
        return _targetTransform != null && 
               _targetController != null && 
               _targetController.isActiveAndEnabled;
    }

    private bool IsTargetInvalid()
    {
        return !IsTargetValid();
    }

    public void Reacquire()
    {
        _nextReacquireTime = Time.time + reacquireInterval;
        _targetTransform = null;
        _targetController = null;

        var playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerController != null && playerController.isActiveAndEnabled)
        {
            _targetController = playerController;
            _targetTransform = playerController.transform;
        }
    }
}
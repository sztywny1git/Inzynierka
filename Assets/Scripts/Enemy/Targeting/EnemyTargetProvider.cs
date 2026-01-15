using UnityEngine;

public class EnemyTargetProvider : MonoBehaviour
{
    [SerializeField] private float reacquireInterval = 0.5f;

    private float _nextReacquireTime;
    private Transform _target;

    public Transform Target
    {
        get
        {
            if (_target == null && Time.time >= _nextReacquireTime)
            {
                Reacquire();
            }
            return _target;
        }
    }

    private void Awake()
    {
        _nextReacquireTime = 0f;
        Reacquire();
    }

    public void Reacquire()
    {
        _nextReacquireTime = Time.time + reacquireInterval;

        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            _target = playerController.transform;
            return;
        }

        var character = FindFirstObjectByType<Character>();
        _target = character != null ? character.transform : null;
    }
}

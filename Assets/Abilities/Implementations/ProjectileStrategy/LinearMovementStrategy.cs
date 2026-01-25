using UnityEngine;

public class LinearMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private float _speed;
    public bool IsDone => false;

    public LinearMovementStrategy() { }

    public void Reset(float speed)
    {
        _speed = speed;
    }

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        if (_transform) _transform.position += _transform.right * _speed * dt;
    }
}
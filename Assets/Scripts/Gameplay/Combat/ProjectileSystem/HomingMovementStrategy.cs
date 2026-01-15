using UnityEngine;

public class HomingMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private readonly Transform _target;
    private readonly float _speed;
    private readonly float _turnSpeed;
    
    public bool IsDone => _target == null || !_target.gameObject.activeInHierarchy;

    public HomingMovementStrategy(Transform target, float speed, float turnSpeed)
    {
        _target = target;
        _speed = speed;
        _turnSpeed = turnSpeed;
    }

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        if (_target)
        {
            Vector3 dir = (_target.position - _transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRot, _turnSpeed * dt);
        }
        _transform.position += _transform.up * _speed * dt;
    }
}
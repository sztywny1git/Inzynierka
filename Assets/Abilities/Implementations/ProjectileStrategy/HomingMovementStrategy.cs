using UnityEngine;

public class HomingMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private Transform _target;
    
    private readonly float _speed;
    private readonly float _turnSpeed;
    private readonly float _searchRadius;
    private readonly LayerMask _layerMask;

    public bool IsDone => false;

    public HomingMovementStrategy(float speed, float turnSpeed, float searchRadius, LayerMask layerMask)
    {
        _speed = speed;
        _turnSpeed = turnSpeed;
        _searchRadius = searchRadius;
        _layerMask = layerMask;
    }

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            Collider2D hit = Physics2D.OverlapCircle(_transform.position, _searchRadius, _layerMask);
            if (hit != null)
            {
                _target = hit.transform;
            }
        }

        if (_target != null)
        {
            Vector3 dir = (_target.position - _transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRot, _turnSpeed * dt);
        }

        _transform.position += _transform.right * _speed * dt;
    }
}
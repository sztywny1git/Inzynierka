using UnityEngine;

public class HomingMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private Transform _target;
    private Collider2D _targetCollider;
    
    private float _speed;
    private float _turnSpeed;
    private float _searchRadius;
    private LayerMask _layerMask;
    private float _homingDelay;

    private float _currentLifetime;

    public bool IsDone => false;

    public HomingMovementStrategy() { }

    public void Reset(float speed, float turnSpeed, float searchRadius, LayerMask layerMask, float homingDelay)
    {
        _speed = speed;
        _turnSpeed = turnSpeed;
        _searchRadius = searchRadius;
        _layerMask = layerMask;
        _homingDelay = homingDelay;
        
        _target = null;
        _targetCollider = null;
        _currentLifetime = 0f;
    }

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        _currentLifetime += dt;
        Vector3 moveDirection = _transform.right;

        if (_currentLifetime >= _homingDelay)
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                FindClosestTarget();
            }

            if (_target != null)
            {
                Vector3 targetPosition = (_targetCollider != null) 
                    ? _targetCollider.bounds.center 
                    : _target.position;

                Vector3 directionToTarget = targetPosition - _transform.position;
                directionToTarget.z = 0;
                directionToTarget.Normalize();

                moveDirection = Vector3.RotateTowards(
                    _transform.right, 
                    directionToTarget, 
                    (_turnSpeed * Mathf.Deg2Rad) * dt, 
                    0.0f
                );
            }
        }

        _transform.right = moveDirection;
        _transform.position += _transform.right * _speed * dt;
    }

    private void FindClosestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_transform.position, _searchRadius, _layerMask);
        
        float closestDistanceSqr = Mathf.Infinity;
        Transform bestTarget = null;
        Collider2D bestCollider = null;

        foreach (var hit in hits)
        {
            Vector3 directionToTarget = hit.transform.position - _transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = hit.transform;
                bestCollider = hit;
            }
        }

        _target = bestTarget;
        _targetCollider = bestCollider;
    }
}
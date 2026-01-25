using UnityEngine;

public class HomingMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private Transform _target;
    private Collider2D _targetCollider; // Cache'ujemy collider, żeby celować w środek
    
    private float _speed;
    private float _turnSpeed;
    private float _searchRadius;
    private LayerMask _layerMask;

    public bool IsDone => false;

    public HomingMovementStrategy() { }

    public void Reset(float speed, float turnSpeed, float searchRadius, LayerMask layerMask)
    {
        _speed = speed;
        _turnSpeed = turnSpeed;
        _searchRadius = searchRadius;
        _layerMask = layerMask;
        _target = null;
        _targetCollider = null;
    }

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        // 1. Znajdź cel, jeśli go nie ma
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            FindClosestTarget();
        }

        // 2. Logika lotu
        Vector3 moveDirection = _transform.right; // Domyślnie lecimy tam, gdzie patrzymy

        if (_target != null)
        {
            // A. Ustal punkt celowania (Środek Collidera, a nie stopy!)
            Vector3 targetPosition = (_targetCollider != null) 
                ? _targetCollider.bounds.center 
                : _target.position;

            Vector3 directionToTarget = targetPosition - _transform.position;
            directionToTarget.z = 0; // Ignorujemy oś Z w 2D
            directionToTarget.Normalize();

            // B. "Magia" wektorowa - Płynny obrót wektora zamiast kątów
            // Vector3.RotateTowards(obecnyKierunek, celKierunek, maxRadianyNaKlatke, magnituda)
            moveDirection = Vector3.RotateTowards(
                _transform.right, 
                directionToTarget, 
                (_turnSpeed * Mathf.Deg2Rad) * dt, // Zamiana stopni na radiany
                0.0f
            );
        }

        // 3. Aplikowanie obrotu i ruchu
        _transform.right = moveDirection; // To automatycznie obraca sprite'a
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
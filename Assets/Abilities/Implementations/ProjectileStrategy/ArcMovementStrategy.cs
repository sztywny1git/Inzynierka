using UnityEngine;

public class ArcMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _duration;
    private float _height;
    private AnimationCurve _curve;
    private float _timer;

    public bool IsDone => _timer >= _duration;

    public ArcMovementStrategy() { }

    public void Reset(Vector3 target, float duration, float height, AnimationCurve curve)
    {
        _targetPosition = target;
        _duration = duration;
        _height = height;
        _curve = curve;
        _timer = 0f;
    }

    public void Initialize(Transform t)
    {
        _transform = t;
        _startPosition = t.position;
    }

    public void Update(float dt)
    {
        _timer += dt;
        float progress = Mathf.Clamp01(_timer / _duration);
        
        Vector3 nextPos = Vector3.Lerp(_startPosition, _targetPosition, progress);
        if (_curve != null) nextPos.y += _curve.Evaluate(progress) * _height;

        Vector3 direction = nextPos - _transform.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        _transform.position = nextPos;
    }
}
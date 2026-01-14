using UnityEngine;
public class ArcMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private Vector3 _startPosition;
    private readonly Vector3 _targetPosition;
    private readonly float _duration;
    private readonly float _height;
    private readonly AnimationCurve _curve;
    private float _timer;

    public bool IsDone => _timer >= _duration;

    public ArcMovementStrategy(Vector3 target, float duration, float height, AnimationCurve curve)
    {
        _targetPosition = target;
        _duration = duration;
        _height = height;
        _curve = curve;
    }

    public void Initialize(Transform t)
    {
        _transform = t;
        _startPosition = t.position;
        _timer = 0f;
    }

    public void Update(float dt)
    {
        _timer += dt;
        float progress = Mathf.Clamp01(_timer / _duration);
        Vector3 pos = Vector3.Lerp(_startPosition, _targetPosition, progress);
        if (_curve != null) pos.y += _curve.Evaluate(progress) * _height;
        _transform.position = pos;
    }
}
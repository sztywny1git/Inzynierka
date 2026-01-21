using UnityEngine;

public class LinearMovementStrategy : IMovementStrategy
{
    private Transform _transform;
    private readonly float _speed;
    public bool IsDone => false;

    public LinearMovementStrategy(float speed) => _speed = speed;

    public void Initialize(Transform t) => _transform = t;

    public void Update(float dt)
    {
        // POPRAWKA: Używamy transform.right (oś X), ponieważ w ProjectileAbility
        // ustawiliśmy transform.right jako kierunek celowania.
        if (_transform) _transform.position += _transform.right * _speed * dt;
    }
}
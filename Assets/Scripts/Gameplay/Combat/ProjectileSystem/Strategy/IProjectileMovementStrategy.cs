using UnityEngine;

public interface IMovementStrategy
{
    void Initialize(Transform projectileTransform);
    void Update(float deltaTime);
    bool IsDone { get; }
}
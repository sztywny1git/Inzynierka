using UnityEngine;

public interface IMovementProvider
{
    Vector2 MovementDirection { get; }
    bool IsMoving { get; }
}
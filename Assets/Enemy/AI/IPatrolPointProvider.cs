using UnityEngine;

public interface IPatrolPointProvider
{
    /// <summary>
    /// Returns a world-space point to move to (2D: Z left unchanged by caller).
    /// </summary>
    bool TryGetNextPoint(Vector3 fromWorldPosition, out Vector3 worldPoint);
}

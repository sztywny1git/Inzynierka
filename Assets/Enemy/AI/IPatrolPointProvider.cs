using UnityEngine;

public interface IPatrolPointProvider
{
    bool TryGetNextPoint(Vector3 fromWorldPosition, out Vector3 worldPoint);
}

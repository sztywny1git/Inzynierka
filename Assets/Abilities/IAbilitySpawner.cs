using UnityEngine;

public interface IAbilitySpawner
{

    T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : PoolableObject;

    void SetPoolContainer(Transform container);
    void ClearPools();
}
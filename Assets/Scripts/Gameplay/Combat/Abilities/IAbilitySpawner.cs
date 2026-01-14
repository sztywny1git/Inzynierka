using UnityEngine;

public interface IAbilitySpawner
{
    GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
    void Despawn(GameObject instance);
}
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AbilitySpawner : IAbilitySpawner
{
    private IObjectResolver _container;
    private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, int> _instanceToPrefabId = new Dictionary<int, int>();

    [Inject]
    public void Construct(IObjectResolver container)
    {
        _container = container;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int prefabId = prefab.GetInstanceID();

        if (!_pools.TryGetValue(prefabId, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools.Add(prefabId, pool);
        }

        GameObject instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            instance = _container.Instantiate(prefab);
            _instanceToPrefabId[instance.GetInstanceID()] = prefabId;
        }

        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetActive(true);

        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        instance.SetActive(false);

        int instanceId = instance.GetInstanceID();
        if (_instanceToPrefabId.TryGetValue(instanceId, out int prefabId))
        {
            if (_pools.TryGetValue(prefabId, out var pool))
            {
                pool.Enqueue(instance);
            }
        }
        else
        {
            Object.Destroy(instance);
        }
    }
}
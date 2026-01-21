using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AbilitySpawner : IAbilitySpawner
{
    private IObjectResolver _container;
    private Transform _poolContainer;
    
    private readonly Dictionary<int, Queue<PoolableObject>> _pools = new Dictionary<int, Queue<PoolableObject>>();
    private readonly Dictionary<int, int> _instanceIdToPrefabId = new Dictionary<int, int>();

    [Inject]
    public void Construct(IObjectResolver container)
    {
        _container = container;
    }

    public void SetPoolContainer(Transform container)
    {
        _poolContainer = container;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : PoolableObject
    {
        if (prefab == null) return null;

        int prefabId = prefab.gameObject.GetInstanceID();

        if (!_pools.TryGetValue(prefabId, out var pool))
        {
            pool = new Queue<PoolableObject>();
            _pools.Add(prefabId, pool);
        }

        PoolableObject instance = null;

        while (pool.Count > 0)
        {
            instance = pool.Dequeue();
            if (instance != null) break;
        }

        if (instance == null)
        {
            instance = _container.Instantiate(prefab, _poolContainer);
            _instanceIdToPrefabId[instance.gameObject.GetInstanceID()] = prefabId;
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        
        instance.ReturnRequested += ReturnToPoolCallback;
        instance.OnSpawn();

        return instance as T;
    }

    private void ReturnToPoolCallback(PoolableObject instance)
    {
        instance.ReturnRequested -= ReturnToPoolCallback;
        instance.OnDespawn();
        instance.gameObject.SetActive(false);

        if (_poolContainer != null)
        {
            instance.transform.SetParent(_poolContainer);
        }

        int instanceId = instance.gameObject.GetInstanceID();
        if (_instanceIdToPrefabId.TryGetValue(instanceId, out int prefabId))
        {
            if (_pools.TryGetValue(prefabId, out var pool))
            {
                pool.Enqueue(instance);
            }
        }
        else
        {
            Object.Destroy(instance.gameObject);
        }
    }

    public void ClearPools()
    {
        _pools.Clear();
        _instanceIdToPrefabId.Clear();
    }
}
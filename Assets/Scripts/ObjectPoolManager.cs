using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    private Dictionary<string, object> _pools = new();

    public ObjectPool<T> CreateObjectPool<T>(T prefab, int initialSize) where T : Component
    {
        var key = prefab.name;
        if (_pools.ContainsKey(key))
        {
            return (ObjectPool<T>)_pools[key];
        }
        
        var pool = new ObjectPool<T>(prefab, initialSize, transform);
        _pools[key] = pool;
        return pool;
    }

    public ObjectPool<T> GetObjectPool<T>(T prefab) where T : Component
    {
        var key = prefab.name;
        if (_pools.ContainsKey(key))
        {
            return (ObjectPool<T>)_pools[key];
        }

        return null;
    }
}
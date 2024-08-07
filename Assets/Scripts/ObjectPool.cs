using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Queue<T> _objects = new();
    private readonly Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (var i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    private void CreateObject()
    {
        var newObject = Object.Instantiate(_prefab, _parent);
        newObject.gameObject.SetActive(false);
        _objects.Enqueue(newObject);
    }

    public T Get()
    {
        if (_objects.Count == 0)
        {
            CreateObject();
        }

        var obj = _objects.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        _objects.Enqueue(obj);
    }
}
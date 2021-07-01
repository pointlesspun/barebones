using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public int Id { get; private set; }
    
    public GameObject ParentObject { get; private set; }

    private List<PoolObject> _available;

    public ObjectPool(int id, int count, GameObject prefab, GameObject parentObject)
    {
        Id = id;
        ParentObject = parentObject;

        _available = new List<PoolObject>();

        for (var i = 0; i < count; i++)
        {
            var obj = GameObject.Instantiate(prefab, parentObject.transform);
            var config = obj.GetComponent<PoolObject>();

            obj.name = parentObject.name + "@" + id + "-" + i;
            

            if (config == null)
            {
                config = obj.AddComponent<PoolObject>();
            }

            config.poolId = Id;
            config.deferRelease = false;
            config.isReleased = true;
            obj.SetActive(false);

            _available.Add(config);
        }
    }

    public PoolObject Obtain()
    {
        if (_available.Count > 0)
        {
            var result = _available[0];
            _available.RemoveAt(0);

            result.gameObject.SetActive(true);
            result.deferRelease = false;
            result.isReleased = false;

            return result;
        }

        return null;
    }

    public void Release(PoolObject meta)
    {
        Debug.Assert(meta.poolId == Id);

        meta.gameObject.SetActive(false);
        meta.isReleased = true;

        _available.Add(meta);
    }
}


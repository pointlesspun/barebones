using System.Collections.Generic;
using UnityEngine;

public class GameObjectMetaPool
{
    public int Id { get; private set; }
    private List<GameObjectMeta> _available;

    public GameObjectMetaPool(int id, int count, GameObject prefab, GameObject parentObject)
    {
        Id = id;

        _available = new List<GameObjectMeta>();

        for (var i = 0; i < count; i++)
        {
            var obj = GameObject.Instantiate(prefab, parentObject.transform);
            var config = obj.GetComponent<GameObjectMeta>();

            obj.name = parentObject.name + "@" + id + "-" + i;
            

            if (config == null)
            {
                config = obj.AddComponent<GameObjectMeta>();
            }

            config.poolId = Id;
            config.deferRelease = false;
            config.isReleased = true;
            obj.SetActive(false);

            _available.Add(config);
        }
    }

    public GameObjectMeta Obtain()
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

    public void Release(GameObjectMeta meta)
    {
        Debug.Assert(meta.poolId == Id);

        meta.gameObject.SetActive(false);
        meta.isReleased = true;

        _available.Add(meta);
    }
}


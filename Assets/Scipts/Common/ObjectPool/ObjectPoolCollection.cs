using System;
using System.Linq;
using UnityEngine;

public enum PoolIdEnum
{
    Players = 0,
    PlayerBullets = 1,
    EnemyDrones = 2,
    None = 3,
}

[Serializable]
public class ObjectPoolConfig
{
    public string name;
    public int size;
    public GameObject prefab;
    public PoolIdEnum preferredId = PoolIdEnum.None;
}

public class ObjectPoolCollection : MonoBehaviour
{
    public static ObjectPoolCollection instance;

    public ObjectPoolConfig[] poolCollectionConfig;

    public ObjectPool this[int index] => poolCollection[index];

    private ObjectPool[] poolCollection;

    public void Awake()
    {
        if (poolCollection == null)
        {
            if (poolCollectionConfig.Length > 0)
            {
                poolCollectionConfig.OrderBy(p => (int) p.preferredId);
                poolCollection = InitializePool(poolCollectionConfig);
            }
            else
            {
                Debug.LogWarning("No custom PoolCollection Configuration defined.");
            }
        }

        instance = this;
    }

    public PoolObject Obtain(int poolId)
    {
        return poolCollection[poolId].Obtain();
    }


    public PoolObject Obtain(int poolId, Transform transform, in Vector3 localStartPosition, in Quaternion rotation)
    {
        var result = poolCollection[poolId].Obtain();

        if (result != null)
        {
            if (transform != null)
            {
                result.gameObject.transform.parent = transform;
            }
            result.gameObject.transform.localPosition = localStartPosition;
            result.gameObject.transform.rotation = rotation;
        }
        else
        {
            Debug.LogWarning("pool " + poolId + ", has run empty.");
        }

        return result;
    }

    public void Release(PoolObject meta)
    {
        var pool = poolCollection[meta.poolId];
        pool.Release(meta);
        meta.gameObject.transform.parent = pool.ParentObject.transform;
    }

    public void OnDestroy()
    {
        instance = null;
    }

    private ObjectPool[] InitializePool(ObjectPoolConfig[] config)
    {
        var result = new ObjectPool[config.Length];

        for (var i = 0; i < config.Length; i++)
        {
            if (config[i].size > 0)
            {
                if (config[i].prefab != null)
                {
                    var poolObject = new GameObject();

                    poolObject.transform.parent = transform;
                    poolObject.name = config[i].name;

                    result[i] = new ObjectPool(i, config[i].size, config[i].prefab, poolObject);
                }
                else
                {
                    Debug.LogError("Pool " + i + ": has a null prefab.");
                }
            }
            else
            {
                Debug.LogError("Pool " + i + ": has size " + config[i].size + ".");
            }
        }

        return result;
    }
}


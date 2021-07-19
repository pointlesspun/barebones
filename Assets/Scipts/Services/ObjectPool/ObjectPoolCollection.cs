using System.Collections.Generic;

using UnityEngine;

using BareBones.Common;

namespace BareBones.Services.ObjectPool
{
    public class ObjectPoolCollection : MonoBehaviour, IObjectPoolCollection
    {
        public ObjectPoolConfig[] poolCollectionConfig;

        public ObjectPool this[int index] => _poolCollection[index];

        private List<ObjectPool> _poolCollection;

        public void Awake()
        {
            var locator = ResourceLocator._instance;

            if (locator.Contains<IObjectPoolCollection>())
            {
                IObjectPoolCollection other = locator.Resolve<IObjectPoolCollection>();
#pragma warning disable CS0252
                if (other != this)
#pragma warning restore CS0252
                {
                    // merge this config with the existing one
                    other.Add(poolCollectionConfig);
                    Destroy(gameObject);
                }
            }
            else
            {
                if (_poolCollection == null)
                {
                    _poolCollection = new List<ObjectPool>();

                    if (poolCollectionConfig.Length > 0)
                    {
                        poolCollectionConfig.OrderBy(p => (int)p.preferredId);
                        Add(poolCollectionConfig);
                    }
                    else
                    {
                        Debug.LogWarning("No custom PoolCollection Configuration defined.");
                    }
                }

                locator.Register<IObjectPoolCollection>(this);
            }
        }

        public PoolObject Obtain(int poolId)
        {
            return _poolCollection[poolId].Obtain();
        }

        public PoolObject Obtain(int poolId, Transform transform, in Vector3 localStartPosition, in Quaternion rotation)
        {
            var result = _poolCollection[poolId].Obtain();

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
            var pool = _poolCollection[meta.poolId];
            pool.Release(meta);
            meta.gameObject.transform.parent = pool.ParentObject.transform;
        }

        public void OnDestroy()
        {
            if (this._poolCollection != null)
            {
                ResourceLocator._instance.Deregister<IObjectPoolCollection>(this);
            }
        }

        public void Add(ObjectPoolConfig[] config)
        {
            for (var i = 0; i < config.Length; i++)
            {
                if (config[i].size > 0)
                {
                    if (config[i].prefab != null)
                    {
                        if (_poolCollection.FindIndex(0, pool => pool.Id == (int)config[i].preferredId) >= 0)
                        {
                            Debug.LogWarning("Pool collection already contains objects for pool id: " + config[i].preferredId);
                            Debug.LogWarning("New pool will be ignored.");
                        }
                        else
                        {
                            _poolCollection.Add(CreateObjectPool(config[i]));
                        }
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

            _poolCollection.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private ObjectPool CreateObjectPool(ObjectPoolConfig config)
        {
            var poolObject = new GameObject();

            poolObject.transform.parent = transform;
            poolObject.name = config.name;

            return new ObjectPool((int)config.preferredId, config.size, config.prefab, poolObject);
        }
    }
}
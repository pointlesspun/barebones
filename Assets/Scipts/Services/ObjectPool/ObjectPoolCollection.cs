using System;
using System.Collections.Generic;

using UnityEngine;

using BareBones.Common;

namespace BareBones.Services.ObjectPool
{
    public class ObjectPoolCollection : MonoBehaviour, IObjectPoolCollection<GameObject>
    {
        public ObjectPoolConfig[] poolCollectionConfig;

        public int PoolCount => _poolCollection.Count;

        public ObjectPool<GameObject> this[int index] => _poolCollection[index];

        public bool _sweep = true;

        public bool _addDebugNamesToPool = false;

        private List<ObjectPool<GameObject>> _poolCollection;

        private Dictionary<int, GameObject> _poolParents;

        public void Awake()
        {
            var locator = ResourceLocator._instance;

            if (locator.Contains<IObjectPoolCollection<GameObject>>())
            {
                IObjectPoolCollection<GameObject> other = locator.Resolve<IObjectPoolCollection<GameObject>>();
#pragma warning disable CS0252
                if (other != this)
#pragma warning restore CS0252
                {
                    // merge this config with the existing one
                    poolCollectionConfig.ForEach(config =>
                           other.AddPool(config._name, MapId(config.preferredId), config._size, config._prefab));

                    Destroy(gameObject);
                }
            }
            else
            {
                locator.Register<IObjectPoolCollection<GameObject>>(this);

                if (_poolCollection == null)
                {
                    _poolCollection = new List<ObjectPool<GameObject>>();
                    _poolParents = new Dictionary<int, GameObject>();

                    if (poolCollectionConfig.Length > 0)
                    {
                        poolCollectionConfig.ForEach(config =>
                           AddPool(config._name, MapId(config.preferredId), config._size, config._prefab, false));
                        _poolCollection.OrderBy(p => p.PoolId);
                    }
                    else
                    {
                        Debug.LogWarning("No custom PoolCollection Configuration defined.");
                    }
                }              
            }
        }

        public void Update()
        {
            if (_sweep)
            {
                Sweep((obj) => !obj.activeInHierarchy);
            }
        }

        public void Sweep(Func<GameObject, bool> predicate)
        {
            for (var i = 0; i < _poolCollection.Count; i++)
            {
                var pool = _poolCollection[i];

                if (pool.Available < pool.Capacity)
                {
                    pool.Sweep(predicate);
                }
            }
        }

        public PoolObjectHandle Obtain(int poolIdx)
        {
            var pool = _poolCollection[poolIdx];
            if (pool.Available > 0)
            {
                var (handle, obj) = pool.Obtain();

                obj.SetActive(true);

                return new PoolObjectHandle()
                {
                    _objectHandle = handle,
                    _poolIdx = poolIdx
                };
            }

            Debug.LogWarning("pool " + poolIdx + ", has run empty.");

            return PoolObjectHandle.NullHandle;
        }

        public void Release(in PoolObjectHandle handle)
        {
            _poolCollection[handle._poolIdx].Release(handle._objectHandle);
        }

        public void OnDestroy()
        {
            if (this._poolCollection != null)
            {
                ResourceLocator._instance.Deregister<IObjectPoolCollection<GameObject>>(this);
            }
        }

        public void RemovePool(int poolIdx, bool destroyGameObjects = true)
        {
            var pool = _poolCollection[poolIdx];
            var poolParent = _poolParents[poolIdx];

            if (destroyGameObjects)
            {
                for (var i = 0; i < pool.Capacity; i++)
                {
                    var obj = pool.GetManagedObject(i);

                    if (obj != null)
                    {
                        if (Application.isPlaying)
                        {
                            GameObject.Destroy(obj);
                        }
                        else 
                        {
                            GameObject.DestroyImmediate(obj);
                        }
                    }
                }

                if (poolParent != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(poolParent);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(poolParent);
                    }
                }

                pool.Clear();
            }
            else
            {
                if (poolParent != null)
                {
                    poolParent.transform.parent = null;
                }
            }

            _poolParents.Remove(poolIdx);
            _poolCollection.RemoveAt(poolIdx);
        }

        public void AddPool(string name, int id, int size, GameObject prefab)
        {
            AddPool(name, id, size, prefab, true);
        }

        public void AddPool(string name, int id, int size, GameObject prefab, bool sortCollectionById)
        {
            if (size > 0)
            {
                if (prefab != null)
                {
                    if (_poolCollection.FindIndex(pool => pool != null && pool.PoolId == id) >= 0)
                    {
                        Debug.LogWarning("Duplicate Id: Pool collection already contains objects for pool id: " + id);
                        Debug.LogWarning("New pool will be ignored.");
                    }
                    else
                    {
                        _poolCollection.Add(CreateObjectPool(name, id, size, prefab));
                    }
                }
                else
                {
                    Debug.LogError("Pool " + id + ": has a null prefab.");
                }
            }
            else
            {
                Debug.LogError("Pool " + id + ": has size " + size + ".");
            }

            if (sortCollectionById)
            {
                _poolCollection.OrderBy((p) => p.PoolId);
            }
        }

        private ObjectPool<GameObject> CreateObjectPool(string name, int id, int size, GameObject prefab)
        {
            var poolObject = new GameObject();

            _poolParents[id] = poolObject;

            poolObject.transform.parent = transform;
            poolObject.name = name;

            if (_addDebugNamesToPool)
            {
                poolObject.AddComponent<ChildCountDebug>();
            }

            return new ObjectPool<GameObject>(
                size, (idx) =>
                {
                    var result = GameObject.Instantiate(prefab);
                    result.transform.parent = poolObject.transform;
                    result.SetActive(false);
                    result.name = prefab.name + "-" + idx;
                    return result;
                },
                (obj) => {
                    
                    if (obj.activeInHierarchy)
                    {
                        obj.SetActive(false);
                    }

                    obj.transform.parent = poolObject.transform;
                },
                id
            );
        }

        private int MapId(PoolIdEnum id)
        {
            if (id < PoolIdEnum.AutoIndex)
            {
                return (int)id;
            }

            var idx = (int)PoolIdEnum.AutoIndex;

            while (_poolCollection.Any( pool => pool.PoolId == idx))
            {
                idx++;
            }

            return idx;
        }
    }
}
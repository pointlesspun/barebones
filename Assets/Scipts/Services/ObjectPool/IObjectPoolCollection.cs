using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    public struct PoolObjectHandle
    {
        public static readonly PoolObjectHandle NullHandle = new PoolObjectHandle()
        {
            gameObject = null,
            objectHandle = -1,
            poolId = -1,
        };

        public int poolId;
        public int objectHandle;
        public GameObject gameObject;
    }

    public interface IObjectPoolCollection
    {
        ObjectPool<GameObject> this[int idx] { get; }

        int PoolCount { get; }

        int GetAvailable(int poolId);

        void AddPool(string name, int id, int size, GameObject prefab);

        void RemovePool(int poolId, bool destroyGameObjects = true);

        PoolObjectHandle Obtain(int poolIdx);

        void Release(in PoolObjectHandle handle);

        bool IsInUse(in PoolObjectHandle handle);
    }
}
using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    public struct PoolObjectHandle
    {
        public int poolId;
        public int objectHandle;
        public GameObject gameObject;
    }

    public interface IObjPoolCollection
    {
        ObjPool<GameObject> this[int idx] { get; }

        int PoolCount { get; }

        int GetAvailable(int poolId);

        void AddPool(string name, int id, int size, GameObject prefab);

        void RemovePool(int poolId, bool destroyGameObjects = true);

        PoolObjectHandle? Obtain(int poolId);

        void Release(in PoolObjectHandle handle);
    }
}
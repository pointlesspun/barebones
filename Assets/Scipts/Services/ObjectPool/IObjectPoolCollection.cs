using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    public interface IObjectPoolCollection
    {
        ObjectPool this[int index] { get; }

        void Add(ObjectPoolConfig[] config);

        PoolObject Obtain(int poolId);

        PoolObject Obtain(int poolId, Transform transform, in Vector3 localStartPosition, in Quaternion rotation);

        void Release(PoolObject meta);
    }
}
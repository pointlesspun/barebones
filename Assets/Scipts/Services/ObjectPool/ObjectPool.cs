using BareBones.Common;
using System;
using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    public enum ObjectPoolState
    {
        Available,
        InUse,
        Released
    }

    [Serializable]

    public class ObjectPool<T> where T : class
    {
        public int VersionMask = 0xffff;
        public int VersionShift = 16;

        private class ObjMetaData
        {
            public T _obj;
            public ObjectPoolState _state;
            public int _version;
            public Action<T> _onRelease;
        }

        private SlotArray<T, ObjMetaData> _pool;

        public int GetVersion(int handle) => (handle >> VersionShift) & VersionMask;

        public int GetSlotHandle(int handle) => handle & VersionMask;

        public int Available => _pool.Capacity - _pool.Available;

        public int Capacity => _pool.Capacity;

        public int PoolId { get; set; } = -1;
        

        public ObjectPool(int count, int id = -1) : this(count, (idx) => Activator.CreateInstance<T>())
        {
            PoolId = id;
        }

        public ObjectPool(int count, Func<int, T> factory, Action<T> onRelease = null, int id = -1)
        {
            _pool = new SlotArray<T, ObjMetaData>(count, (idx) => new ObjMetaData());
            PoolId = id;

            for (var i = 0; i < count; i++)
            {
                var slot = _pool.Assign(factory(i));
                var meta = _pool.GetMetaData(slot);
                meta._obj = _pool[slot];
                meta._state = ObjectPoolState.Available;
                meta._version = 0;
                meta._onRelease = onRelease;
            }
        }

        public ObjectPool(params T[] values)
        {
            _pool = new SlotArray<T, ObjMetaData>(values.Length, (idx) => new ObjMetaData());

            for (var i = 0; i < values.Length; i++)
            {
                var slot = _pool.Assign(values[i]);
                var meta = _pool.GetMetaData(slot);
                meta._obj = _pool[slot];
                meta._state = ObjectPoolState.Available;
                meta._version = 0;
            }
        }

        public ObjectPoolState GetState(int handle)
        {
            return _pool.GetMetaData(GetSlotHandle(handle))._state;
        }

        public bool IsInUse(int handle)
        {
            var meta = _pool.GetMetaData(GetSlotHandle(handle));

            return meta._state == ObjectPoolState.InUse && GetVersion(handle) == meta._version;
        }

        public T Dereference(int handle)
        {
            var meta = _pool.GetMetaData(GetSlotHandle(handle));
            if ((meta._state == ObjectPoolState.InUse || meta._state == ObjectPoolState.Released)
                && GetVersion(handle) == meta._version)
            {
                return meta._obj;
            }

            Debug.LogError("Dereferencing object " + meta._obj + " with handle " + handle + " which is no longer in use or  the version does not match.");

            return default(T);
        }

        public void Sweep(Func<T, bool> shouldRelease)
        {
            for (var i = _pool.FirstAvailable; i != -1;)
            {
                var meta = _pool.GetMetaData(i);                
                var next = _pool.Next(i);

                switch (meta._state)
                {
                    case ObjectPoolState.InUse:
                        if (shouldRelease(meta._obj))
                        {
                            meta._state = ObjectPoolState.Released;
                        }
                        break;
                    case ObjectPoolState.Released:
                        Release(meta, i);
                        break;
                }

                i = next;
            }
        }

        public (int handle, T obj) Obtain()
        {
            Debug.Assert(_pool.Available < _pool.Capacity);

            var poolId = _pool.First;
            var meta = _pool.GetMetaData(poolId);
            
            Debug.Assert(meta._state == ObjectPoolState.Available);

            meta._state = ObjectPoolState.InUse;
            meta._version = (meta._version + 1) & VersionMask;

            //Debug.Log("obtained: " + poolId + "(" + _pool.GetSlot(poolId) + ") , version" + meta._version);

            return (handle: poolId | (meta._version << VersionShift), 
                            obj: _pool.Release(poolId));
        }

        public void Clear()
        {
            _pool.Clear();

            for (var i = 0; i < _pool.Capacity; i++)
            {
                var meta = _pool.GetMetaData(i);
                _pool.Assign(meta._obj);
                
                meta._state = ObjectPoolState.Available;
                meta._version++;
            }
        }

        public void Release(int handle, ObjectPoolState state = ObjectPoolState.Available)
        {
            Debug.Assert(state == ObjectPoolState.Released || state == ObjectPoolState.Available);

            var poolId = GetSlotHandle(handle);
            
            Debug.Assert(poolId >= 0 && poolId < _pool.Capacity);

            var meta = _pool.GetMetaData(poolId);

            if (meta._version == GetVersion(handle) && meta._state != ObjectPoolState.Available)
            {
                if (state == ObjectPoolState.Available)
                {
                    Release(meta, poolId);
                }

                _pool.GetMetaData(poolId)._state = state;
            }
            else
            {
                Debug.LogError("ObjPool.Release Fail. PoolId: " + poolId + ", version (expected version: " + meta._version + " handle version:" + GetVersion(handle) +
                        ") or object state ( expected state: " + ObjectPoolState.Available + " actual state " + meta._state + " ) incorrect, object will not be released");
            }
        }

        public T GetManagedObject(int idx)
        {
            return _pool.GetMetaData(idx)._obj;
        }


        private void Release(ObjMetaData meta, int poolId)
        {
            meta._version = (meta._version + 1) & VersionMask;
            meta._state = ObjectPoolState.Available;
            
            if (meta._onRelease != null)
            {
                meta._onRelease(meta._obj);
            }

            _pool.Assign(meta._obj, poolId);

            //Debug.Log("released: " + poolId + "(" + _pool.GetSlot(poolId) + "), version" + meta._version);
        }
    }
}



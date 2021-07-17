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

    public class ObjPool<T> where T : class
    {
        public int VersionMask = 0xffff;
        public int VersionShift = 16;

        private class ObjMetaData
        {
            public T _obj;
            public ObjectPoolState _state;
            public int _version;
        }

        private SlotArray<T, ObjMetaData> _pool;

        public int GetVersion(int handle) => (handle >> VersionShift) & VersionMask;

        public int GetSlotHandle(int handle) => handle & VersionMask;

        public int Available => _pool.Capacity - _pool.Available;

        public int Capacity => _pool.Capacity;

        public ObjPool(int count) : this(count, (idx) => Activator.CreateInstance<T>())
        {
        }

        public ObjPool(int count, Func<int, T> factory)
        {
            _pool = new SlotArray<T, ObjMetaData>(count, (idx) => new ObjMetaData());

            for (var i = 0; i < count; i++)
            {
                var slot = _pool.Assign(factory(i));
                var meta = _pool.GetMetaData(slot);
                meta._obj = _pool[slot];
                meta._state = ObjectPoolState.Available;
                meta._version = 0;
            }
        }

        public ObjPool(params T[] values)
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

        public (int handle, T obj) Obtain()
        {
            Debug.Assert(_pool.Available < _pool.Capacity);

            var poolId = _pool.First;
            var meta = _pool.GetMetaData(poolId);
            
            Debug.Assert(meta._state == ObjectPoolState.Available);

            var result = (handle: poolId | (meta._version << VersionShift), 
                            obj: _pool.Release(poolId));

            //Debug.Log("obtained: " + poolId + "(" + _pool.GetSlot(poolId) + ") , version" + meta._version);

            meta._state = ObjectPoolState.InUse;
            return result;
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

            if (meta._version == GetVersion(handle)
                && meta._state == ObjectPoolState.InUse)
            {
                if (state == ObjectPoolState.Available)
                {
                    meta._version = (meta._version + 1) & VersionMask;
                    _pool.Assign(meta._obj, poolId);
                    //Debug.Log("released: " + poolId + "(" + _pool.GetSlot(poolId) + "), version" + meta._version);
                }

                _pool.GetMetaData(poolId)._state = state;
            }
            else
            {
                Debug.LogError("ObjPool.Release Fail. PoolId: " + poolId + ", version (expected version: " + meta._version + " handle version:" + GetVersion(handle) +
                        ") or object state ( expected state: " + ObjectPoolState.Available + " actual state " + meta._state + " ) incorrect, object will not be released");
            }
        }
    }
}



using System;

namespace BareBones.Services.ObjectPool
{
    /**
     * Contains a collection of object pools where each objectpool manages
     * a specifc prefab. The general use is:
     * 
     *  - add one or more pools via 'AddPool()'
     *  - obtain gameobject handles via 'Obtain()'
     *  - deref the handle to get access to the actual game object via 'Dereference()'
     *  - either release objects via 'Release()' or call 'Sweep()' 
     *    back into its respective pool
     */
    public interface IObjectPoolCollection<T> where T : class
    {
        /** Returns the pool in the given index */
        ObjectPool<T> this[int idx] { get; }

        /** Returns the number of pools */
        int PoolCount { get; }
        
        /** Add a pool to the collection */
        void AddPool(string name, int idx, int size, T prefab);

        /** Remove a pool from the collection */
        void RemovePool(int poolIdx, bool destroyObjects = true);

        /** Object an object from the given pool */
        PoolObjectHandle Obtain(int poolIdx);

        /** Iterates over all pools releasing objects which match the predicate */
        void Sweep(Func<T, bool> predicate);

        /** Release an object back to its pool */
        void Release(in PoolObjectHandle handle);
    }   
}
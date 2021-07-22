using System;
using System.Collections.Generic;

namespace BareBones.Services.ObjectPool
{
    public static class IObjectPoolCollectionExtensions
    {
        /** Maps the given pool id to a index in the poolCollection */
        public static int FindPoolIdx<T>(this IObjectPoolCollection<T> poolCollection, PoolIdEnum poolId) where T : class
        {
            for (var i = 0; i < poolCollection.PoolCount; i++)
            {
                if (poolCollection[i].PoolId == (int)poolId)
                {
                    return i;
                }
            }

            return -1;
        }

        /** Gets the available objects in the given pool */
        public static int GetAvailable<T>(this IObjectPoolCollection<T> poolCollection, int poolIdx) where T : class
        {
            return poolCollection[poolIdx].Available;
        }

        /** Derefence the handle obtaining the game object iff the handle is valid */
        public static T Dereference<T>(this IObjectPoolCollection<T> poolCollection, in PoolObjectHandle handle) where T : class
        {
            return handle._poolIdx >= 0 && handle._poolIdx < poolCollection.PoolCount
                ? poolCollection[handle._poolIdx].Dereference(handle._objectHandle)
                : default;
        }

        /** Checks if the object with the given handle is in use */
        public static bool IsInUse<T>(this IObjectPoolCollection<T> poolCollection, in PoolObjectHandle handle) where T : class
        {
            return poolCollection[handle._poolIdx].IsInUse(handle._objectHandle);
        }

        /**
         * Enumerates data in the given pools. Note that the readonly is not enforced in any real way,
         * it's just a suggestion of how to use this.
         */
        public static IEnumerable<T> ReadOnlyEnumeration<T>(this IObjectPoolCollection<T> poolCollection, params int[] poolIndices)
            where T : class
        {
            for (var i = 0; i < poolIndices.Length; i++)
            {
                foreach (var t in poolCollection[poolIndices[i]])
                {
                    yield return t;
                }
            }
        }

        /**
         * Enumerates data in the all the pools. Note that the readonly is not enforced in any real way,
         * it's just a suggestion of how to use this.
         */
        public static IEnumerable<T> ReadOnlyEnumeration<T>(this IObjectPoolCollection<T> poolCollection)
            where T : class
        {
            for (var i = 0; i < poolCollection.PoolCount; i++)
            {
                foreach (var t in poolCollection[i])
                {
                    yield return t;
                }
            }
        }

        /** A handle to the first gameobject in the collection matching the predicate */
        public static PoolObjectHandle First<T>(this IObjectPoolCollection<T> poolCollection, Func<T, bool> predicate)
            where T : class
        {
            for (var i = 0; i < poolCollection.PoolCount; i++)
            {
                var pool = poolCollection[i];
                for (var j = pool.First; j != -1; j = pool.Next(j))
                {
                    if (predicate(pool.Read(j))) 
                    {
                        return poolCollection.GetHandle(i, j);
                    }
                }
            }

            return PoolObjectHandle.NullHandle;
        }

        /** A handle to the first gameobject in the pools in collection matching the predicate */
        public static PoolObjectHandle First<T>(this IObjectPoolCollection<T> poolCollection, Func<T, bool> predicate, params int[] poolIds)
            where T : class
        {
            for (var i = 0; i < poolIds.Length; i++)
            {
                var pool = poolCollection[poolIds[i]];
                for (var j = pool.First; j != -1; j = pool.Next(j))
                {
                    if (predicate(pool.Read(j)))
                    {
                        return poolCollection.GetHandle(poolIds[i], j);
                    }
                }
            }

            return PoolObjectHandle.NullHandle;
        }

        /** Find the object with the highest evaluation according the given evaluate function */
        public static PoolObjectHandle FindBest<T>(this IObjectPoolCollection<T> poolCollection, Func<T, float> evaluate, params int[] poolIds)
            where T : class
        {
            var currentBestPool = -1;
            var currentBestIdx = -1;
            var bestValue = float.MinValue;

            for (var i = 0; i < poolIds.Length; i++)
            {
                var pool = poolCollection[poolIds[i]];
                for (var j = pool.First; j != -1; j = pool.Next(j))
                {
                    var value = evaluate(pool.Read(j));

                    if (value > bestValue)
                    {
                        bestValue = value;
                        currentBestIdx = j;
                        currentBestPool = poolIds[i];
                    }
                }
            }

            return poolCollection.GetHandle(currentBestPool, currentBestIdx);
        }


        /** Fill the buffer with all gameobjects in the pools in collection matching the predicate */
        public static int Where<T>(this IObjectPoolCollection<T> poolCollection, Func<T, bool> predicate, PoolObjectHandle[] buffer, params int[] poolIds)
            where T : class
        {
            var bufferIdx = 0;

            for (var i = 0; i < poolIds.Length; i++)
            {
                var pool = poolCollection[poolIds[i]];
                for (var j = pool.First; j != -1; j = pool.Next(j))
                {
                    if (predicate(pool.Read(j)))
                    {
                        buffer[bufferIdx] = poolCollection.GetHandle(poolIds[i], j);
                        bufferIdx++;

                        if (bufferIdx >= buffer.Length)
                        {
                            return bufferIdx; 
                        }
                    }
                }
            }

            return bufferIdx;
        }

        /** Fill the buffer with all gameobjects in the collection matching the predicate */
        public static int Where<T>(this IObjectPoolCollection<T> poolCollection, Func<T, bool> predicate, PoolObjectHandle[] buffer)
            where T : class
        {
            var bufferIdx = 0;

            for (var i = 0; i < poolCollection.PoolCount; i++)
            {
                var pool = poolCollection[i];
                for (var j = pool.First; j != -1; j = pool.Next(j))
                {
                    if (predicate(pool.Read(j)))
                    {
                        buffer[bufferIdx] = poolCollection.GetHandle(i, j);
                        bufferIdx++;

                        if (bufferIdx >= buffer.Length)
                        {
                            return bufferIdx;
                        }
                    }
                }
            }

            return bufferIdx;
        }
    }
}

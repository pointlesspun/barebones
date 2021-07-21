namespace BareBones.Services.ObjectPool
{
    public static class IObjectPoolCollectionExtensions
    {
        /** Maps the given pool id to a index in the poolCollection */
        public static int FindPoolIdx<T>(this IObjectPoolCollection<T> poolCollection, PoolIdEnum poolId) where T: class
        {
            for (var i = 0; i < poolCollection.PoolCount; i++ )
            {
                if (poolCollection[i].PoolId == (int) poolId)
                {
                    return i;
                }
            }

            return -1;
        }

        /** Gets the available objects in the given pool */
        public static int GetAvailable<T>(this IObjectPoolCollection<T> poolCollection, int poolIdx) where T:class
        {
            return poolCollection[poolIdx].Available;
        }

        /** Derefence the handle obtaining the game object iff the handle is valid */
        public static T Dereference<T>(this IObjectPoolCollection<T> poolCollection, in PoolObjectHandle handle) where T : class
        {
            return poolCollection[handle._poolIdx].Dereference(handle._objectHandle);
        }

        /** Checks if the object with the given handle is in use */
        public static bool IsInUse<T>(this IObjectPoolCollection<T> poolCollection, in PoolObjectHandle handle) where T : class
        {
            return poolCollection[handle._poolIdx].IsInUse(handle._objectHandle);
        }
    }
}

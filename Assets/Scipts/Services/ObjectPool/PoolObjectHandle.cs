namespace BareBones.Services.ObjectPool
{
    /**
     * Handle to an object in an IObjectPoolCollection
     */
    public struct PoolObjectHandle
    {
        /** Value returned when no more objects are available in a poolcollection */
        public static readonly PoolObjectHandle NullHandle = new PoolObjectHandle()
        {
            _objectHandle = -1,
            _poolIdx = -1,
        };

        /** Index of the pool in an IObjectPoolCollection */
        public int _poolIdx;

        /** Handle used to refer to a gameObject in an ObjectPool */
        public int _objectHandle;

        /** Checks if this handle has a reference to a valid object */
        public bool HasReference => _objectHandle >= 0;
    }
}
using System;

using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    /**
     * End user description of an object pool
     */ 
    [Serializable]
    public class ObjectPoolConfig
    {
        /** (Human readable) Name of the pool */
        public string _name;

        /** Size of the pool (number of objects) */
        public int _size;

        /** Prefab used to fill the objects in the pool */
        public GameObject _prefab;

        /** Id of the pool */
        public PoolIdEnum preferredId = PoolIdEnum.AutoIndex;
    }
}
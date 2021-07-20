using System;

using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    [Serializable]
    public class ObjectPoolConfig
    {
        public string name;
        public int size;
        public GameObject prefab;
        public PoolIdEnum preferredId = PoolIdEnum.AutoIndex;
    }
}
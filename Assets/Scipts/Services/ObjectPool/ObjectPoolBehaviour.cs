
using UnityEngine;

using BareBones.Common;


public class ObjectPoolBehaviour : MonoBehaviour
{
    public PoolIdEnum Id = PoolIdEnum.Other; 

    public void Awake()
    {
        var locator = ResourceLocator._instance;
    }
}


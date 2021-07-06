#define USE_OBJECT_POOLS

using System.Collections.Generic;
using UnityEngine;

public class DirectedWeapon : MonoBehaviour, IWeapon
{
    public PoolIdEnum bulletPoolId;  

    public int bulletsPerShot = 1;

    public float cooldown = 0.25f;

    private float lastFiredBullet = -1.0f;

    private IObjectPoolCollection _pool;

    public void Start()
    {
        _pool = ResourceLocator._instance.Resolve<IObjectPoolCollection>();
    }

    public void Fire()
    {
        Fire(gameObject.transform, gameObject.transform.position);
    }

    public void Fire(in Vector3 localStartPosition)
    {
        Fire(null, gameObject.transform.position);
    }

    public void Fire(Transform transform, in Vector3 localStartPosition)
    {
        if (Time.time - lastFiredBullet > cooldown)
        {
            _pool.Obtain(
                (int)bulletPoolId,
                transform,
                localStartPosition,
                gameObject.transform.rotation
            );
            
            lastFiredBullet = Time.time;
        }
    }
}

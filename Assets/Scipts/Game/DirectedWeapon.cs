using UnityEngine;

using BareBones.Common;
using BareBones.Common.Behaviours;
using BareBones.Services.ObjectPool;

namespace BareBones.Game
{
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
                var handle = _pool.Obtain((int)bulletPoolId);

                if (handle.gameObject != null)
                {
                    var obj = handle.gameObject;

                    obj.transform.localPosition = localStartPosition;
                    obj.transform.rotation = gameObject.transform.rotation;
                    obj.transform.parent = gameObject.transform.transform;
                }
                
                lastFiredBullet = Time.time;
            }
        }
    }
}
using UnityEngine;

using BareBones.Common;
using BareBones.Services.ObjectPool;

namespace BareBones.Game
{
    public class DirectedWeapon : MonoBehaviour, IWeapon
    {
        public PoolIdEnum _bulletPoolId;

        public int _bulletsPerShot = 1;

        public float _cooldown = 0.25f;

        private float _lastFiredBullet = -1.0f;

        private IObjectPoolCollection<GameObject> _poolCollection;
        private int _bulletPoolIdx;

        public void Start()
        {
            _poolCollection = ResourceLocator._instance.Resolve<IObjectPoolCollection<GameObject>>();

            Debug.Assert(_poolCollection != null, "Expected to find a IObjectPoolCollection<GameObject> declared in the ResourceLocator.");

            _bulletPoolIdx = _poolCollection.FindPoolIdx(_bulletPoolId);

            Debug.Assert(_bulletPoolIdx != -1, "No pool with pool id " + _bulletPoolId + " declared in the object poolCollection.");
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
            if (Time.time - _lastFiredBullet > _cooldown)
            {
                var handle = _poolCollection.Obtain(_bulletPoolIdx);

                if (handle.HasReference)
                {
                    var obj = _poolCollection.Dereference(handle);

                    obj.transform.localPosition = localStartPosition;
                    obj.transform.rotation = gameObject.transform.rotation;
                    obj.transform.parent = gameObject.transform.transform;
                }
                
                _lastFiredBullet = Time.time;
            }
        }
    }
}
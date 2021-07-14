using UnityEngine;

using BareBones.Common;

namespace BareBones.Services.ObjectPool
{
    public class PoolObject : MonoBehaviour
    {
        public int poolId;
        public bool isReleased = false;
        public bool deferRelease = false;

        private IObjectPoolCollection _pool;

        public void Start()
        {
            _pool = ResourceLocator._instance.Resolve<IObjectPoolCollection>();
        }

        public void Release()
        {
            if (deferRelease)
            {
                isReleased = true;
                gameObject.SetActive(false);
            }
            else
            {
                _pool.Release(this);
            }
        }
    }
}
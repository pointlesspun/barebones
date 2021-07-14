using UnityEngine;

using BareBones.Common;
using BareBones.Common.Messages;

namespace BareBones.Game
{
    public class Hitpoints : MonoBehaviour
    {
        public float baselineHitpoints = 1;

        public float currentHitpoints = 1;

        public bool isInvulnerable = false;

        public bool deferDestruction = false;

        private PoolObject _meta;

        private IMessageBus _messageBus;

        void OnEnable()
        {
            currentHitpoints = baselineHitpoints;
        }

        void Start()
        {
            _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
            _meta = GetComponent<PoolObject>();
        }

        public void OnHit(float damage)
        {
            if (!isInvulnerable)
            {
                currentHitpoints -= damage;

                if (currentHitpoints <= 0)
                {
                    _messageBus.Send(MessageTopics.Entity, MessageIds.EntityDestroyed, gameObject, null);

                    if (_meta != null)
                    {
                        _meta.Release();
                    }
                    else if (!deferDestruction)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
using UnityEngine;

using BareBones.Common;
using BareBones.Services.Messages;

namespace BareBones.Game
{
    public class Hitpoints : MonoBehaviour
    {
        public float baselineHitpoints = 1;

        public float currentHitpoints = 1;

        public bool isInvulnerable = false;

        private IMessageBus _messageBus;

        void OnEnable()
        {
            currentHitpoints = baselineHitpoints;
        }

        void Start()
        {
            _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
        }

        public void OnHit(float damage)
        {
            if (!isInvulnerable)
            {
                currentHitpoints -= damage;

                if (currentHitpoints <= 0)
                {
                    _messageBus.Send(MessageTopics.Entity, MessageIds.EntityDestroyed, gameObject, null);

                    // xxx assumption here is this object is pooled and will be cleaned up 
                    // the objectpool's sweep
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
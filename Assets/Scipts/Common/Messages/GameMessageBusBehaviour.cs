using UnityEngine;

namespace BareBones.Common.Messages
{

    public class GameMessageBusBehaviour : MonoBehaviour
    {
        private GameMessageBus _eventBus;
        private ResourceLocator _locator;

        public void Awake()
        {
            _locator = ResourceLocator._instance;

            if (_eventBus == null && !_locator.Contains<IGameMessageBus>())
            {
                _eventBus = _locator.Register<IGameMessageBus, GameMessageBus>();
            }
        }

        public void Update()
        {
            if (_eventBus != null)
            {
                _eventBus.Update();
            }
        }

        public void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Clear();
                _locator.Deregister<IGameMessageBus>(_eventBus);
                _eventBus = null;
            }
        }
    }
}
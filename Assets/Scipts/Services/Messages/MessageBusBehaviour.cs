using UnityEngine;

using BareBones.Common;

namespace BareBones.Services.Messages
{
    public class MessageBusBehaviour : MonoBehaviour
    {
        public int _messageSize = 32;
        public int _listenerSize = 32;

        public bool _logActivity = false;

        private MessageBus _messageBus;
        private ResourceLocator _locator;

        public void Awake()
        {
            _locator = ResourceLocator._instance;

            if (_messageBus == null && !_locator.Contains<IMessageBus>())
            {
                _messageBus = _locator.Register<IMessageBus, MessageBus>(_messageSize, _listenerSize);
                _messageBus.debugLog = _logActivity;
            }
        }

        public void Update()
        {
            if (_messageBus != null)
            {
                _messageBus.Update();
            }
        }

        public void OnDestroy()
        {
            if (_messageBus != null)
            {
                _messageBus.ClearMessages();
                _locator.Deregister<IMessageBus>(_messageBus);
                _messageBus = null;
            }
        }
    }
}
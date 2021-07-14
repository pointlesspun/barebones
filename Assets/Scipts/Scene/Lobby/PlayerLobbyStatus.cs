
using UnityEngine;

using BareBones.Common;
using BareBones.Services.Messages;

namespace BareBones.Scene.Lobby
{
    public class PlayerLobbyStatus : MonoBehaviour, IMessageListener
    {
        private IMessageBus _messageBus;
        private int _listenerHandle;

        public void OnEnable()
        {
            if (_messageBus == null)
            {
                _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
            }

            _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Player);
        }

        public void OnDisable()
        {
            if (_messageBus != null && _listenerHandle != -1)
            {
                _messageBus.Unsubscribe(_listenerHandle);
            }
        }

        public void HandleMessage(Message message)
        {
            var playerIndex = (int)message.payload;

            if (playerIndex >= 0 && transform.childCount > playerIndex)
            {
                transform.GetChild(playerIndex).gameObject.SetActive(message.id == MessageIds.PlayerJoined);
            }
            else
            {
                Debug.LogWarning("Player index (" + playerIndex + ") is outside the range of children the PlayerLobbyStatus can affect. Make sure there the number of children matches the max number of players.");
            }
        }
    }
}
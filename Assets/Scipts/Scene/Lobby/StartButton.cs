using System;

using UnityEngine;
using UnityEngine.UI;

using BareBones.Common;
using BareBones.Services.Messages;

namespace BareBones.Scene.Lobby
{
    public class StartButton : MonoBehaviour, IMessageListener
    {
        public string waitingForPlayersText = "Waiting for players...";
        public string startText = "Start !!!";

        private IMessageBus _messageBus;
        private int _listenerHandle;

        private int _playersRegistered = 0;

        private Button _button;
        private TMPro.TMP_Text _buttonText;

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

        void Start()
        {
            _button = GetComponent<Button>();

            _buttonText = transform.GetComponentInChildren<TMPro.TMP_Text>();

            if (_buttonText != null)
            {
                _buttonText.text = waitingForPlayersText;
            }
        }


        public void HandleMessage(Message message)
        {
            // xxx this needs refinement
            _playersRegistered += message.id == MessageIds.PlayerJoined ? 1 : -1;

            var allPlayersReady = _playersRegistered > 0;

            _button.interactable = allPlayersReady;

            if (_buttonText != null)
            {
                _buttonText.text = allPlayersReady
                    ? startText
                    : waitingForPlayersText;
            }
        }
    }

}
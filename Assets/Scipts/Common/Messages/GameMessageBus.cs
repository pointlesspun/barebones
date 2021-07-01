using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Common.Messages
{

    [Serializable]
    public class GameMessageBus : IGameMessageBus
    {
        public const int BUFFER_SIZE = 32;

        public int ReadBufferLength => _readBufferLength;

        private GameMessage[] _readBuffer;
        private GameMessage[] _writeBuffer;

        private int _readBufferLength = 0;
        private int _writeBufferLength = 0;

        private readonly List<IGameMessageListener> _listeners = new List<IGameMessageListener>();

        public GameMessageBus()
        {
            _readBufferLength = 0;
            _writeBufferLength = 0;

            _readBuffer = new GameMessage[BUFFER_SIZE];
            _writeBuffer = new GameMessage[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                _readBuffer[i] = new GameMessage();
                _writeBuffer[i] = new GameMessage();
            }
        }

        public void AddListener(IGameMessageListener listener)
        {
            Debug.Assert(!_listeners.Contains(listener), "Duplicate call to AddListener, listener = " + listener);
            _listeners.Add(listener);
        }

        public void RemoveListener(IGameMessageListener listener)
        {
            Debug.Assert(_listeners.Contains(listener), "Removing listener which was not registered, listener = " + listener);
            _listeners.Remove(listener);
        }

        public void Update()
        {
            if (_listeners.Count > 0 && _readBufferLength > 0)
            {
                try
                {
                    UpdateListeners();
                }
                catch (Exception e)
                {
                    Debug.LogError("GameMessageBus.Update: Exception while updating listeners.");
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
            }

            SwitchReadWriteBuffers();
        }

        public GameMessage Send(int messageId, GameObject sender, System.Object payload)
        {
            var evt = Obtain();

            if (evt != null)
            {
                evt.messageId = messageId;
                evt.sender = sender;
                evt.payload = payload;
            }

            return evt;
        }

        public void Clear()
        {
            _readBufferLength = 0;
            _writeBufferLength = 0;
        }

        public GameMessage Read(int index) => _readBuffer[index];

        private GameMessage Obtain()
        {
            if (_writeBufferLength < BUFFER_SIZE)
            {
                var result = _writeBuffer[_writeBufferLength];

                result.sender = null;
                result.messageId = -1;
                result.payload = null;

                _writeBufferLength++;

                return result;
            }

            Debug.LogError("Event write buffer has been exhausted");
            return null;
        }

        private void UpdateListeners()
        {
            var evtCount = ReadBufferLength;
            var listenerCount = _listeners.Count;

            for (var i = 0; i < evtCount; i++)
            {
                var evt = _readBuffer[i];

                for (var j = 0; j < listenerCount; j++)
                {
                    var listener = _listeners[j];

                    if ((evt.messageId & listener.GameMessageFlags) > 0)
                    {
                        listener.HandleMessage(evt);
                    }
                }
            }
        }

        private void SwitchReadWriteBuffers()
        {
            var temp = _readBuffer;

            _readBuffer = _writeBuffer;
            _writeBuffer = temp;

            _readBufferLength = _writeBufferLength;
            _writeBufferLength = 0;
        }
    }
}
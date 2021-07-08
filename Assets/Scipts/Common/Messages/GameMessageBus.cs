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
        private readonly List<IGameMessageListener> _stagedListeners = new List<IGameMessageListener>();
        
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
            Debug.Assert(listener.GameMessageListenerState == GameMessageListenerState.None, "Duplicate call to AddListener, listener = " + listener);

            // can't immediately add a listener to the active listener lists if the addition is the result
            // of a message being send and handled by another listener. So stage the listener
            // which will be added next update
            _stagedListeners.Add(listener);
            listener.GameMessageListenerState = GameMessageListenerState.Staged;
        }

        /**
         * This will flag the listener for removal but doesn't actually remove it from 
         * the lists of listeners.
         */
        public void RemoveListener(IGameMessageListener listener)
        {
            Debug.Assert(listener.GameMessageListenerState != GameMessageListenerState.None, "Removing listener which was not registered, listener = " + listener);
            Debug.Assert(listener.GameMessageListenerState != GameMessageListenerState.FlaggedForRemoval, "Duplicate removal call to listener = " + listener);

            // if the handle is negative, the listener was in the staging queue.
            // remove it from the queue.
            if (listener.GameMessageListenerState == GameMessageListenerState.Staged)
            {
                _stagedListeners.Remove(listener);
                listener.GameMessageListenerState = GameMessageListenerState.None;
            }
            else
            {
                // by setting the handle to 0 we flag that this listener is ready for removal if
                // it is an active listener or just clear the handle if it was staged. 
                listener.GameMessageListenerState = GameMessageListenerState.FlaggedForRemoval;
            }
        }

        public void Update()
        {
            if (_stagedListeners.Count > 0 || (_listeners.Count > 0 && _readBufferLength > 0))
            {
                try
                {
                    AddStagedListeners();
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

        public GameMessage Send(GameMessageCategories category, int messageId, GameObject sender, System.Object payload)
        {
            var message = Obtain();

            if (message != null)
            {
                message.messageCategory = category;
                message.messageId = messageId;
                message.sender = sender;
                message.payload = payload;
            }

            return message;
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
                result.messageCategory = GameMessageCategories.Any;
                result.messageId = -1;
                result.payload = null;

                _writeBufferLength++;

                return result;
            }

            Debug.LogError("Event write buffer has been exhausted");
            return null;
        }

        private void AddStagedListeners()
        {
            for (var i = 0; i < _stagedListeners.Count; i++)
            {
                _listeners.Add(_stagedListeners[i]);
                _stagedListeners[i].GameMessageListenerState = GameMessageListenerState.Active;
            }

            _stagedListeners.Clear();
        }

        private void UpdateListeners()
        {
            var messageCount = ReadBufferLength;

            for (var i = 0; i < messageCount; i++)
            {
                var evt = _readBuffer[i];

                for (var j = 0; j < _listeners.Count;)
                {
                    var listener = _listeners[j];

                    if (listener == null || listener.GameMessageListenerState != GameMessageListenerState.Active)
                    {
                        _listeners.RemoveAt(j);

                        if (listener != null)
                        {
                            listener.GameMessageListenerState = GameMessageListenerState.None;
                        }
                    }
                    else 
                    {
                        if ((evt.messageCategory & listener.CategoryFlags) > 0)
                        {
                            listener.HandleMessage(evt);
                        }

                        j++;
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
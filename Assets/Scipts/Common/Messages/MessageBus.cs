using System;
using UnityEngine;

namespace BareBones.Common.Messages
{

    [Serializable]
    public class MessageBus : IMessageBus
    {
        private enum MessageBusListenerState
        {
            None,
            Staged,
            Active,
            FlaggedForRemoval
        }

        private class ListenerMetaData
        {
            public MessageBusListenerState state = MessageBusListenerState.None;
            public int topicFlags = 0;
        }

        public const int MESSAGE_BUFFER_SIZE = 32;
        public const int MAX_LISTENERS = 32;

        public int MessageCount => _readBufferLength;

        private Message[] _readBuffer;
        private Message[] _writeBuffer;

        private int _readBufferLength = 0;
        private int _writeBufferLength = 0;

        private SlotArray<IMessageListener, ListenerMetaData> _listeners;
        private bool _isUpdating = false;
        private int modifiedListeners = 0;

        public int ListenerCount => _listeners.Count;

        public MessageBus(int messageBufferSize = MESSAGE_BUFFER_SIZE, int maxListeners = MAX_LISTENERS)
        {
            _readBufferLength = 0;
            _writeBufferLength = 0;

            _readBuffer = new Message[messageBufferSize];
            _writeBuffer = new Message[messageBufferSize];

            for (int i = 0; i < messageBufferSize; i++)
            {
                _readBuffer[i] = new Message();
                _writeBuffer[i] = new Message();
            }

            _listeners = new SlotArray<IMessageListener, ListenerMetaData>(maxListeners, (idx) => new ListenerMetaData());
        }

        public int Subscribe(IMessageListener listener, int topicFlags)
        {
            Debug.Assert(listener != null);
            Debug.Assert(!Contains(listener), "Duplicate call to AddListener, listener = " + listener);

            // can't immediately add a listener to the active listener lists if the addition is the result
            // of a message being send and handled by another listener. So stage the listener
            // which will be added next update
            var handle = _listeners.Assign(listener);

            if (handle >= 0)
            {
                var meta = _listeners.GetMetaData(handle);
                meta.topicFlags = topicFlags;
                if (_isUpdating)
                {
                    modifiedListeners++;
                    meta.state = MessageBusListenerState.Staged;
                }
                else
                {
                    meta.state = MessageBusListenerState.Active;
                }
            }
            else
            {
                Debug.LogWarning("MessageBus.Subscribe: no more slots for listeners.");
            }
            

            return handle;
        }

        /**
         * This will flag the listener for removal but doesn't actually remove it from 
         * the lists of listeners.
         */
        public void Unsubscribe(int handle)
        {
            var meta = _listeners.GetMetaData(handle);

            Debug.Assert(meta.state != MessageBusListenerState.None, "Removing listener which was not registered, listener handle = " + handle);
           
            if (_isUpdating)
            {
                if (meta.state != MessageBusListenerState.Staged)
                {
                    modifiedListeners++;
                }
                meta.state = MessageBusListenerState.FlaggedForRemoval;
            }
            else
            { 
                meta.state = MessageBusListenerState.None;
                _listeners.Release(handle);
            }
        }

        public void Update()
        {
            if (_listeners.Count > 0 || _readBufferLength > 0)
            {
                _isUpdating = true;

                try
                {
                    var idx = _listeners.First;

                    while (idx != -1)
                    {
                        var meta = _listeners.GetMetaData(idx);
                        var listener = _listeners[idx];
                        var next = _listeners.Next(idx);

                        // object may have been deleted
                        if (listener == null && meta.state != MessageBusListenerState.FlaggedForRemoval)
                        {
                            Debug.LogWarning("Listener (" + listener + ") was deleted while not flagged for removal... removing automatically.");
                            meta.state = MessageBusListenerState.FlaggedForRemoval;
                        }

                        switch (meta.state)
                        {
                            case MessageBusListenerState.Active:
                                HandleMessages(listener, meta);
                                break;
                            case MessageBusListenerState.FlaggedForRemoval:
                                meta.state = MessageBusListenerState.None;
                                _listeners.Release(idx);
                                modifiedListeners--;
                                break;
                            case MessageBusListenerState.Staged:
                                meta.state = MessageBusListenerState.Active;
                                modifiedListeners--;
                                break;
                            default:
                                Debug.Break();
                                Debug.LogError("Listener (" + listener + ") with unknown or None state encountered... removing.");
                                _listeners.Release(idx);
                                break;
                        }

                        idx = next;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("GameMessageBus.Update: Exception while updating listeners.");
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }

                _isUpdating = false;
            }

            if (modifiedListeners > 0)
            {
                UpdateModifiedListeners(modifiedListeners);
                modifiedListeners = 0;
            }
            
            SwitchReadWriteBuffers();
        }

        private void UpdateModifiedListeners(int count)
        {
            var updatedListeners = 0;

            for (var idx = _listeners.First; idx != -1 && updatedListeners < count; idx = _listeners.Next(idx))
            {
                var meta = _listeners.GetMetaData(idx);
                if (meta.state == MessageBusListenerState.Staged)
                {
                    meta.state = MessageBusListenerState.Active;
                    updatedListeners++;
                }
                else if (meta.state == MessageBusListenerState.FlaggedForRemoval)
                {
                    meta.state = MessageBusListenerState.None;
                    _listeners.Release(idx);
                    updatedListeners++;
                }
            }
        }

        public bool Send(int topic, int messageId, System.Object sender, System.Object payload)
        {
            var message = Obtain();

            if (message != null)
            {
                message.Initialize(topic, messageId, sender, payload);
            }

            return message != null;
        }

        public void ClearMessages()
        {
            _readBufferLength = 0;
            _writeBufferLength = 0;
        }

        public bool Contains(IMessageListener listener)
        {
            for (var idx = _listeners.First; idx != -1; idx = _listeners.Next(idx))
            {
                if (_listeners[idx] == listener)
                {
                    return true;
                }
            }

            return false;
        }

        public Message Read(int index) => index < _readBufferLength ? new Message(_readBuffer[index]) : null;

        private Message Obtain()
        {
            if (_isUpdating)
            {
                if (_writeBufferLength < _writeBuffer.Length)
                {
                    var result = _writeBuffer[_writeBufferLength].Initialize();
                    _writeBufferLength++;
                    return result;
                }

                Debug.LogWarning("MessageBus write buffer has been exhausted");
                return null;
            }
            else
            {
                if (_readBufferLength < _readBuffer.Length)
                {
                    var result = _readBuffer[_readBufferLength].Initialize();
                    _readBufferLength++;
                    return result;
                }

                Debug.LogWarning("MessageBus read buffer has been exhausted");
                return null;
            }
        }

        private void HandleMessages(IMessageListener listener, ListenerMetaData meta)
        {
            var messageCount = MessageCount;

            for (var i = 0; i < messageCount && meta.state == MessageBusListenerState.Active; i++)
            {
                var message = _readBuffer[i];

                if ((message.topic & meta.topicFlags) > 0)
                {
                    listener.HandleMessage(message);
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
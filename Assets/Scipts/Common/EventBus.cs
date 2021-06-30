using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventBus : IEventBus
{
    public const int BUFFER_SIZE = 32;

    public int ReadBufferLength => _readBufferLength;

    private GameEvent[] _readBuffer;
    private GameEvent[] _writeBuffer;

    private int _readBufferLength = 0;
    private int _writeBufferLength = 0;

    private readonly List<IGameEventListener> _listeners = new List<IGameEventListener>();

    public EventBus()
    {
        _readBufferLength = 0;
        _writeBufferLength = 0;

        _readBuffer = new GameEvent[BUFFER_SIZE];
        _writeBuffer = new GameEvent[BUFFER_SIZE];

        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            _readBuffer[i] = new GameEvent();
            _writeBuffer[i] = new GameEvent();
        }
    }

    public void AddListener(IGameEventListener listener)
    {
        Debug.Assert(!_listeners.Contains(listener), "Duplicate call to AddListener, listener = " + listener);
        _listeners.Add(listener);
    }

    public void RemoveListener(IGameEventListener listener)
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
                Debug.LogError(e);
            }
        }

        SwitchReadWriteBuffers();
    }

    public GameEvent Send(int eventId, GameObject sender, System.Object payload)
    {
        var evt = Obtain();

        if (evt != null)
        {
            evt.eventId = eventId;
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

    public GameEvent Read(int index) => _readBuffer[index];

    private GameEvent Obtain()
    {
        if (_writeBufferLength < BUFFER_SIZE)
        {
            var result = _writeBuffer[_writeBufferLength];

            result.sender = null;
            result.eventId = -1;
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

                if ((evt.eventId & listener.GameEventFlags) > 0)
                {
                    listener.HandleGameEvent(evt);
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

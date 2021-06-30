using UnityEngine;

public interface IEventBus
{
    int ReadBufferLength { get; }

    void Clear();

    void AddListener(IGameEventListener listener);

    void RemoveListener(IGameEventListener listener);

    GameEvent Send(int eventId, GameObject sender, System.Object payload);

    GameEvent Read(int index);
}


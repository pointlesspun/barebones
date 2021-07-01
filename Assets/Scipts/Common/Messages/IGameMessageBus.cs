﻿using UnityEngine;

namespace BareBones.Common.Messages
{

    public interface IGameMessageBus
    {
        int ReadBufferLength { get; }

        void Clear();

        void AddListener(IGameMessageListener listener);

        void RemoveListener(IGameMessageListener listener);

        GameMessage Send(int eventId, GameObject sender, System.Object payload);

        GameMessage Read(int index);
    }

}
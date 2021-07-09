﻿using UnityEngine;

namespace BareBones.Common.Messages
{

    public interface IGameMessageBus
    {
        int ReadBufferLength { get; }

        void Clear();

        void AddListener(IGameMessageListener listener);

        
        void RemoveListener(IGameMessageListener listener);

        //int Subscribe(IGameMessageListener listener);

        //void Unsubscribe(int handle);


        GameMessage Send(GameMessageCategories category, int id, GameObject sender, System.Object payload);

        GameMessage Read(int index);
    }

}
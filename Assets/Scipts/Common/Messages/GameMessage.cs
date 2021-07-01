
using System;
using UnityEngine;

namespace BareBones.Common.Messages
{
    [Serializable]
    public class GameMessage
    {
        public GameObject sender;
        public int messageId;
        public System.Object payload;
    }
}
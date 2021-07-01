
using System;
using UnityEngine;

namespace BareBones.Common.Messages
{
    [Serializable]
    public class GameMessage
    {
        public GameObject sender;
        public GameMessageCategories messageCategory;
        public int messageId;
        public System.Object payload;
    }
}

using System;

namespace BareBones.Services.Messages
{
    [Serializable]
    public class Message
    {
        public Object sender;
        public int topic;
        public int id;
        public Object payload;

        public Message()
        {
        }

        public Message(Message other)
        {
            Initialize(other);
        }

        public Message Initialize(int topic = 0, int id = -1, Object sender = null, Object payload = null)
        {
            this.topic = topic;
            this.id = id;
            this.sender = sender;
            this.payload = payload;

            return this;
        }

        public Message Initialize(Message other)
        {
            topic = other.topic;
            id = other.id;
            sender = other.sender;
            payload = other.payload;

            return this;
        }

        public override string ToString()
        {
            return "(topic=" + topic + ", id=" + id + ", sender=" + sender + ", payload=" + payload + ")";
        }
    }
}
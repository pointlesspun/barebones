namespace BareBones.Common.Messages
{
    public interface IMessageListener
    {       
        void HandleMessage(Message message);
    }
}


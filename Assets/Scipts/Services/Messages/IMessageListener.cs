namespace BareBones.Services.Messages
{
    public interface IMessageListener
    {       
        void HandleMessage(Message message);
    }
}



namespace BareBones.Common.Messages
{
    public interface IMessageBus
    {
        int MessageCount { get; }

        void ClearMessages();

        int Subscribe(IMessageListener listener, int topicFlags);

        void Unsubscribe(int handle);

        Message Send(int category, int id, System.Object sender, System.Object payload);

        Message Read(int index);
    }
}
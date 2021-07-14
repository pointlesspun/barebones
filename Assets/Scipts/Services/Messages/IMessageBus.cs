
namespace BareBones.Services.Messages
{
    public interface IMessageBus
    {
        /**
         * Number of processed messages in the queue.
         */
        int MessageCount { get; }

        /**
         * Removes all messages, processed and otherwise
         */
        void ClearMessages();

        /**
         * Subscribe the given listener to this message bus
         * @return a handle to the listener in the message bus or -1 if no more slots
         * for listeners were available
         */
        int Subscribe(IMessageListener listener, int topicFlags);

        /**
         * Unsubscribe the listener with the given handle from  this message bus
         */
        void Unsubscribe(int handle);

        /**
         * Send a message with the given values. It's up to the implementation when this message
         * is being processed and delivered to any listeners and in which order.
         * @return true if the message is added for processing false otherwise.
         */
        bool Send(int category, int id, System.Object sender, System.Object payload);

        /**
         * Read a processed message in the queue. 
         * @return a message in the queue at the given index or null if no messages are available in this slot.
         * Note that the returned message is not guanteed to be a reference to the message in the queue but
         * may be a copy.
         */
        Message Read(int index);
    }
}
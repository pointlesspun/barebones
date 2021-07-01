namespace BareBones.Common.Messages
{

    public interface IGameMessageListener
    {
        int GameMessageFlags { get; }

        void HandleMessage(GameMessage evt);
    }
}


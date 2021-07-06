namespace BareBones.Common.Messages
{

    public interface IGameMessageListener
    {
        GameMessageCategories CategoryFlags { get; }

        void HandleMessage(GameMessage message);
    }
}


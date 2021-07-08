namespace BareBones.Common.Messages
{

    public enum GameMessageListenerState
    {
        None,
        Staged,
        Active,
        FlaggedForRemoval
    }

    public interface IGameMessageListener
    {
        GameMessageListenerState GameMessageListenerState { get; set; }

        GameMessageCategories CategoryFlags { get; }

        void HandleMessage(GameMessage message);
    }
}


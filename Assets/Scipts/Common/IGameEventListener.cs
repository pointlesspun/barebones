public interface IGameEventListener
{
    int GameEventFlags { get; }

    void HandleGameEvent(GameEvent evt);
}


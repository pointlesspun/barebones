public static class GameEventIds
{
    // generic event ids
    public const int None = 0;
    public const int PlayerJoined = 1 << 0;
    public const int PlayerCanceled = 1 << 1;

    // in game event ids
    public const int EntityDestroyed = 1 << 8;
}


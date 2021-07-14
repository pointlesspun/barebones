namespace BareBones.Services.Messages
{
    public static class MessageTopics
    {
        public const int All = ~0;
        public const int Player = 1 << 0;
        public const int Scene = 1 << 1;
        public const int Entity = 1 << 2;
    }

    public static class MessageIds
    {
        // lobby events
        public static readonly int PlayerJoined = 1;
        public static readonly int PlayerCanceled = 2;
        public static readonly int PlayerDied = 3;

        // scene events
        public static readonly int SessionStarted = 100;
        public static readonly int SceneStarted = 101;
        public static readonly int SceneEnded = 102;
        public static readonly int SessionEnded = 103;

        // in game events
        public static readonly int EntityDestroyed = 200;
    }
}

using System;

namespace BareBones.Common.Messages
{
    [Flags]
    public enum GameMessageCategories
    {
        Any    = ~0,
        Player = 1 << 0,
        Scene  = 1 << 1,
        Entity = 1 << 8,
    }

    public static class GameMessageIds
    {
        // lobby events
        public static readonly int PlayerJoined = 1;
        public static readonly int PlayerCanceled = 2;

        // scene events
        public static readonly int SessionStarted = 50;
        public static readonly int SceneStarted = 51;
        public static readonly int SceneEnded = 52;
        public static readonly int SessionEnded = 53;

        // in game events
        public static readonly int EntityDestroyed = 100;
    }
}


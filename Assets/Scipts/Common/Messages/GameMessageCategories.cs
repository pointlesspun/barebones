
using System;

namespace BareBones.Common.Messages
{
    [Flags]
    public enum GameMessageCategories
    {
        Any    = ~0,
        Player = 1 << 0,
        Entity = 1 << 8,
    }

    public static class GameMessageIds
    {
        // generic event ids
        public static readonly int PlayerJoined = 1;
        public static readonly int PlayerCanceled = 2;

        public static readonly int EntityDestroyed = 100;
    }
}


using System;
using System.Collections.Generic;

public interface IPlayerRegistry<TPlayer> : IEnumerable<TPlayer> where TPlayer : class
{
    TPlayer this[int index] { get; }

    int AvailableSlots { get; }
    int MaxPlayers { get; }
    int PlayerCount { get; }

    int RegisterPlayer(TPlayer root);

    TPlayer DeregisterPlayer(int playerIndex);


    bool HasPlayerRegistered(TPlayer root);

    bool HasPlayerRegistered(Func<TPlayer, bool> predicate);
}


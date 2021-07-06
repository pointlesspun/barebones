using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerRegistry : IEnumerable<PlayerRoot>
{
    PlayerRoot this[int index] { get; }

    int AvailableSlots { get; }
    int MaxPlayers { get; }
    int PlayerCount { get; }

    int GetAvailableIndex();

    int RegisterPlayer(PlayerRoot root);

    GameObject DeregisterPlayer(int playerIndex);

    PlayerRoot GetPlayer(int playerIndex);

    void Reset();

    bool HasPlayerRegistered(PlayerRoot root);

    bool HasPlayerRegistered(Func<PlayerRoot, bool> predicate);
}


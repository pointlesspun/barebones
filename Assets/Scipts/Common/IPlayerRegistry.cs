using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerRegistry
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


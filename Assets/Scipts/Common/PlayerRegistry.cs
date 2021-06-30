using System;
using UnityEngine;

[Serializable]
public class PlayerRegistry : IPlayerRegistry
{
    public int MaxPlayers { get; private set; } = 3;

    public int PlayerCount => playerCount;

    public int AvailableSlots => MaxPlayers - playerCount;

    public PlayerRoot this[int index] => players[index];

    private int playerCount = 0;

    private PlayerRoot[] players;

    public PlayerRegistry()
    {
        players = new PlayerRoot[MaxPlayers];
    }

    public PlayerRegistry(int maxPlayers)
    {
        players = new PlayerRoot[maxPlayers];
        MaxPlayers = maxPlayers;
    }

    public int GetAvailableIndex()
    {
        return players.GetAvailableSlot();
    }

    public int RegisterPlayer(PlayerRoot root)
    {
        Debug.Assert(!HasPlayerRegistered(root));
        Debug.Assert(PlayerCount < MaxPlayers);

        if (PlayerCount < MaxPlayers)
        {
            var index = GetAvailableIndex();
            players[index] = root;
            playerCount++;

            return PlayerCount - 1;
        }

        return -1;
    }

    public PlayerRoot GetPlayer(int playerIndex)
    {
        return players[playerIndex];
    }


    public GameObject DeregisterPlayer(int playerIndex)
    {
        var obj = players[playerIndex].gameObject;

        players[playerIndex] = null;
        playerCount--;

        return obj;
    }

    public bool HasPlayerRegistered(Func<PlayerRoot, bool> predicate)
    {
        return Array.FindIndex(players, root => root != null && predicate(root)) >= 0;
    }

    public bool HasPlayerRegistered(PlayerRoot root)
    {
        return Array.FindIndex(players, playerRoot => playerRoot == root) >= 0;
    }

    public void Reset()
    {
        players.ForEach(plr =>
        {
            if (plr != null)
            {
                GameObject.Destroy(plr.gameObject);
            }

            return null;
        });

        playerCount = 0;
    }
}


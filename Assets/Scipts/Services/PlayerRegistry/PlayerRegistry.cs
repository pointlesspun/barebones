using System;
using System.Collections;
using System.Collections.Generic;

using BareBones.Common;

namespace BareBones.Services.PlayerRegistry
{
    [Serializable]
    public class PlayerRegistry<TPlayer> : IPlayerRegistry<TPlayer> where TPlayer : class
    {
        private SlotArray<TPlayer, object> _players;

        public int MaxPlayers => _players.Capacity;

        public int PlayerCount => _players.Capacity - _players.Available;

        public int AvailableSlots => _players.Available;

        public TPlayer this[int index] => _players[index];


        public PlayerRegistry(int maxPlayers)
        {
            _players = new SlotArray<TPlayer, object>(maxPlayers);
        }
        public int RegisterPlayer(TPlayer root)
        {
            return _players.Assign(root);
        }

        public TPlayer DeregisterPlayer(int playerIndex)
        {
            var result = _players[playerIndex];

            _players.Release(playerIndex);

            return result;
        }

        public bool HasPlayerRegistered(Func<TPlayer, bool> predicate)
        {
            return _players.FindHandle(root => root != null && predicate(root)) >= 0;
        }

        public bool HasPlayerRegistered(TPlayer root)
        {
            return _players.FindHandle(root) >= 0;
        }

        public IEnumerator<TPlayer> GetEnumerator()
        {
            return _players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
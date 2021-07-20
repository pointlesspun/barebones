using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace BareBones.Common
{
    [Serializable]
    public class SlotArray<TData, TMetaData> : IEnumerable<TData> where TData : class
    {
        public class Slot
        {
            public TMetaData _metaData;
            public TData _data;
            public int _next = -1;
            public int _previous = -1;

            public override string ToString()
            {
                return _previous + " <- " + _data + " -> " + _next;
            }
        }

        private Slot[] _slots;
        private int _firstInUse;
        private int _firstAvailable;
        private int _available;

        public int Capacity => _slots.Length;

        public int Available => _available;

        public int Count => _slots.Length - Available;

        public int First => _firstInUse;

        public int FirstAvailable => _firstAvailable;

        public TData this[int idx] => _slots[idx]._data;

        public SlotArray(int capacity) : this(capacity, (idx) => default(TMetaData))
        {
        }

        public SlotArray(int capacity, Func<int, TMetaData> metaDataConstructor)
        {
            _slots = Enumerations.CreateArray<Slot>(capacity, (idx) =>
            {
                return new Slot()
                {
                    _next = idx + 1,
                    _previous = idx - 1,
                    _metaData = metaDataConstructor(idx)
                };
            });
            _available = capacity;
            _firstInUse = -1;

            _slots[capacity - 1]._next = -1;
        }

        public void Clear()
        {
            _available = _slots.Length;
            _firstAvailable = 0;
            _firstInUse = -1;

            for (var i = 0; i < _slots.Length; i++)
            {
                _slots[i]._next = i + 1;
                _slots[i]._previous = i - 1;
                _slots[i]._data = default;
            }

            _slots[_slots.Length - 1]._next = -1;
        }

        public int Assign(TData data)
        {
            return _available > 0
                ? Assign(data, _firstAvailable)
                : -1;
        }

        public int Assign(TData data, int slotIdx)
        {
            Debug.Assert(slotIdx >= 0 && slotIdx < _slots.Length);
            Debug.Assert(!IsInUse(slotIdx));

            var slot = _slots[slotIdx];

            if (slotIdx == _firstAvailable)
            {
                _firstAvailable = slot._next;

                if (_firstAvailable != -1)
                {
                    _slots[_firstAvailable]._previous = -1;
                }
            }

            if (slot._next != -1)
            {
                _slots[slot._next]._previous = slot._previous;
            }

            if (slot._previous != -1)
            {
                _slots[slot._previous]._next = slot._next;
            }

            slot._data = data;
            slot._next = _firstInUse;
            slot._previous = -1;

            if (_firstInUse != -1)
            {
                _slots[_firstInUse]._previous = slotIdx;
            }

            _firstInUse = slotIdx;

            _available--;

            return slotIdx;
        }

        public TData Release(int handle)
        {
            Debug.Assert(handle >= 0 && handle < _slots.Length);
            Debug.Assert(_slots[handle] != null);

            var slot = _slots[handle];

            if (_firstInUse == handle)
            {
                _firstInUse = slot._next;
            }

            if (slot._next != -1)
            {
                _slots[slot._next]._previous = slot._previous;
            }

            if (slot._previous != -1)
            {
                _slots[slot._previous]._next = slot._next;
            }

            if (_firstAvailable != -1)
            {
                _slots[_firstAvailable]._previous = handle;
            }

            var result = slot._data;

            slot._next = _firstAvailable;
            slot._previous = -1;
            slot._data = default;

            _firstAvailable = handle;

            _available++;

            return result;
        }

        public IEnumerator<TData> GetEnumerator()
        {
            var idx = _firstInUse;

            while (idx != -1)
            {
                var slot = _slots[idx];
                yield return slot._data;
                idx = slot._next;
            }
        }

        public TMetaData GetMetaData(int idx) => _slots[idx]._metaData;

        public Slot GetSlot(int idx) => _slots[idx];

        public int Next(int idx) => _slots[idx]._next;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsInUse(int idx)
        {
            if (idx >= 0 && idx < _slots.Length)
            {
                var i = _firstInUse;

                while (i != -1 && i != idx)
                {
                    i = _slots[i]._next;
                }
                return i == idx;
            }

            return false;
        }
    }

    public static class SlotArrayExtensions
    {
        public static int FindHandle<TData, TMeta>(this SlotArray<TData, TMeta> array, TData data) where TData : class
        {
            for (var idx = array.First; idx != -1; idx = array.Next(idx))
            {
                if (array[idx] == data)
                {
                    return idx;
                }
            }

            return -1;
        }

        public static int FindHandle<TData, TMeta>(this SlotArray<TData, TMeta> array, Func<TData, bool> predicate) where TData : class
        {
            for (var idx = array.First; idx != -1; idx = array.Next(idx))
            {
                if (predicate(array[idx]))
                {
                    return idx;
                }
            }

            return -1;
        }
    }
}
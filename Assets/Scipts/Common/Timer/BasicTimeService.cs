using System;
using System.Collections.Generic;


[Serializable]
public class BasicTimeService : ITimeService
{
    private TimeoutCallbackEntry[] _timers;
    private readonly List<TimeoutCallbackEntry> _timersInUse = new List<TimeoutCallbackEntry>();
    private int _available;
    private int _firstAvailable;

    private float _time;

    public BasicTimeService(int count, float startTime)
    {
        _timers = Enumerations.CreateArray<TimeoutCallbackEntry>(count);
        _available = count; 
        _time = startTime;
        _firstAvailable = 0;
    }

    public void Update(float deltaTime)
    {
        _time += deltaTime;

        for (var i = 0; i < _timersInUse.Count; )
        {
            var timer = _timersInUse[i];
            if (timer.IsTimeout(_time))
            {
                timer.Callback.OnTimeout(timer.Handle);

                timer.Duration = -1;
                timer.Handle = -1;
                timer.Callback = null;

                _timersInUse.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    public int SetTimeout(ITimeoutCallback callback, float duration)
    {
        if (_available > 0 && duration > 0)
        {
            for (var i = 0; i < _timers.Length; i++)
            {
                var idx = (i + _firstAvailable) % _timers.Length;

                if (_timers[idx].Duration <= 0)
                {
                    _timers[idx].Initialize(idx, _time, duration, callback);
                    _available--;
                    _firstAvailable = (idx + 1) % _timers.Length;
                    _timersInUse.Add(_timers[idx]);
                    return idx;
                }
            }
        }

        return -1;
    }

    public void Cancel(int handle)
    {
        if (_timers[handle].Duration > 0)
        {
            for (var i = 0; i < _timersInUse.Count; i++)
            {
                if (_timersInUse[i].Handle == handle)
                {
                    _timers[handle].Duration = -1;
                    _timers[handle].Callback = null;
                    _timers[handle].Handle = -1;
                    _timersInUse.RemoveAt(i);
                    break;
                }
            }
        }
    }
}


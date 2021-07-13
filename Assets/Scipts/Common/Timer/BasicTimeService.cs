using System;


[Serializable]
public class BasicTimeService : ITimeService
{
    private float _time;
    private bool _isUpdating = false;

    private SlotArray<ITimeoutCallback, TimeoutCallbackMetaData> _timers;

    public int Available => _timers.Available;

    public BasicTimeService(int count, float startTime)
    {
        _timers = new SlotArray<ITimeoutCallback, TimeoutCallbackMetaData>(count, (idx) => new TimeoutCallbackMetaData());
        _time = startTime;
    }

    public void Update(float deltaTime)
    {
        _isUpdating = true;
        
        _time += deltaTime;

        var idx = _timers.First;

        while (idx != -1)
        {
            var next = _timers.Next(idx);
            var meta = _timers.GetMetaData(idx);

            // if duration was set to negative number, it implies
            // the timer was canceled while updating in this
            // case skip the check and release the current time
            if (meta.Duration > 0)
            {
                if (meta.IsTimeout(_time))
                {
                    _timers[idx].OnTimeout(idx);

                    meta.Duration = -1;

                    _timers.Release(idx);
                }
            }
            else
            {
                _timers.Release(idx);
            }

            idx = next;
        }

        _isUpdating = false; 
    }

    public int SetTimeout(ITimeoutCallback callback, float duration)
    {
        if (_timers.Available > 0 && duration > 0)
        {
            var idx = _timers.Assign(callback);
            var meta = _timers.GetMetaData(idx);
            meta.Duration = duration;
            meta.StartTime = _time;

            return idx;
        }
           
        return -1;
    }

    public void Cancel(int handle)
    {
        if (_isUpdating)
        {
            // timer will be cleaned up during the next update
            _timers.GetMetaData(handle).Duration = -1;
        }
        else
        {
            _timers.Release(handle);
        }
    }
}


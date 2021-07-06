using System;

[Serializable]
public class TimeoutCallbackEntry
{
    public float Duration
    {
        get;
        set;
    }

    public float StartTime
    {
        get;
        set;
    }

    public ITimeoutCallback Callback
    {
        get;
        set;
    }

    public int Handle
    {
        get;
        set;
    }

    public void Initialize(int handle, float time, float duration, ITimeoutCallback callback)
    {
        Callback = callback;
        Duration = duration;
        Handle = handle;
        StartTime = time;
    }

    public bool IsTimeout(float time) => time > (StartTime + Duration);

}


using UnityEngine;

public class TimeServiceBehaviour : MonoBehaviour
{
    public int timerSlotCount = 8;
    private BasicTimeService _timeService;

    public void Awake()
    {
        if (_timeService == null && !ResourceLocator._instance.Contains<ITimeService>())
        {
            _timeService = ResourceLocator._instance.Register <ITimeService, BasicTimeService> (timerSlotCount, 0);
        }
    }

    public void Update()
    {
        _timeService.Update(Time.deltaTime);
    }

    public int SetTimeout(ITimeoutCallback callback, float duration)
    {
        return _timeService.SetTimeout(callback, duration);
    }

    public void Cancel(int handle)
    {
        _timeService.Cancel(handle);
    }

    public void OnDestroy()
    {
        if (_timeService != null)
        {
            ResourceLocator._instance.Deregister<ITimeService>(_timeService);
        }
    }
}


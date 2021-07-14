namespace BareBones.Services.TimeService
{
    public interface ITimeoutCallback
    {
        void OnTimeout(int handle);
    }
}


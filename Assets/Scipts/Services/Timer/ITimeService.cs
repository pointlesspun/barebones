
namespace BareBones.Services.TimeService
{
    public interface ITimeService
    {
        int SetTimeout(ITimeoutCallback callback, float duration);

        void Cancel(int handle);

        void Update(float deltaTime);
    }

}

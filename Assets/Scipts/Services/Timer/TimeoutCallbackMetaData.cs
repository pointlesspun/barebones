using System;

namespace BareBones.Services.TimeService
{

    [Serializable]
    public class TimeoutCallbackMetaData
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


        public void Initialize(float time, float duration)
        {
            Duration = duration;
            StartTime = time;
        }

        public bool IsTimeout(float time) => time >= (StartTime + Duration);
    }

}
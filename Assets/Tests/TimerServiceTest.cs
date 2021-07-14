using System;
using NUnit.Framework;

using BareBones.Services.TimeService;

public class TimerServiceTest
{
    class TestCallback : ITimeoutCallback
    {
        public Action<int> _callbackAction;

        public TestCallback(Action<int> action)
        {
            _callbackAction = action;

        }
        public void OnTimeout(int handle)
        {
            _callbackAction(handle);
        }
    }

    [Test]
    public void ConstructorTest()
    {
        var timeService = new BasicTimeService(2, 0);

        Assert.IsTrue(timeService.Available == 2);
    }

    [Test]
    public void SetTimeoutTest()
    {
        var timeService = new BasicTimeService(1, 0);

        var handle = timeService.SetTimeout(new TestCallback(handle => { }), 1);
        Assert.IsTrue(timeService.Available == 0);
        Assert.IsTrue(handle >= 0);
        Assert.IsTrue(timeService.SetTimeout(new TestCallback(handle => { }), 1) == -1);
    }

    [Test]
    public void CancelTest()
    {
        var timeService = new BasicTimeService(1, 0);

        var handle = timeService.SetTimeout(new TestCallback(handle => { }), 1);

        timeService.Cancel(handle);

        Assert.IsTrue(timeService.Available == 1);
        handle = timeService.SetTimeout(new TestCallback(handle => { }), 1);
        Assert.IsTrue(handle >= 0);
        Assert.IsTrue(timeService.SetTimeout(new TestCallback(handle => { }), 1) == -1);
    }

    [Test]
    public void UpdateTest()
    {
        var timeService = new BasicTimeService(1, 0);
        var timeOutCallbackCount = 0;
        var handle = timeService.SetTimeout(new TestCallback(handle => { timeOutCallbackCount++; }), 1);

        timeService.Update(0.5f);

        Assert.IsTrue(timeOutCallbackCount == 0);
        Assert.IsTrue(timeService.Available == 0);

        timeService.Update(0.5f);

        Assert.IsTrue(timeOutCallbackCount == 1);
        Assert.IsTrue(timeService.Available == 1);
    }

    [Test]
    public void CancelWhileUpdateTest()
    {
        var timeService = new BasicTimeService(2, 0);
        var timeOutCallbackCount = 0;

        var handle1 = -1;
        var handle2 = -1;

        // know in which timeouts are registered in this case matters for this test
        handle2 = timeService.SetTimeout(new TestCallback(handle => { timeOutCallbackCount++; }), 2);
        handle1 = timeService.SetTimeout(new TestCallback(handle => { timeService.Cancel(handle2); }), 1);

        timeService.Update(2.0f);

        Assert.IsTrue(timeOutCallbackCount == 0);
        Assert.IsTrue(timeService.Available == 2);
    }
}

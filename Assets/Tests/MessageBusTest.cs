using System.Collections.Generic;
using System.Linq;
using BareBones.Common.Messages;
using NUnit.Framework;
using UnityEngine;

public class MessageBusTest
{
    [Test]
    public void ConstructorTest()
    {
        var bus = new MessageBus();

        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);
    }

    [Test]
    public void TestSend()
    {
        var bus = new MessageBus();
        var sender = new Object();
        var payLoad = new Object();
       
        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);

        bus.Send(42, 1, sender, payLoad);
        Assert.IsTrue(bus.MessageCount == 1);

        var message = bus.Read(0);
        Assert.IsTrue(((Object)message.sender) == sender);
        Assert.IsTrue(((Object)message.payload) == payLoad);
        Assert.IsTrue(message.topic == 42);
        Assert.IsTrue(message.id == 1);

        Assert.IsTrue(bus.Read(1) == null);

        bus.Update();

        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);
    }

    class TestListener : IMessageListener
    {
        public Message _message = new Message();

        public void HandleMessage(Message message)
        {
            _message.Initialize(message);
        }
    }

    [Test]
    public void TestSendSubscribe()
    {
        var bus = new MessageBus();
        var sender = new Object();
        var payLoad = new Object();
        var listener = new TestListener();

        bus.Subscribe(listener, 42);
        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(((System.Object)listener._message.sender) == sender);
    }

    [Test]
    public void TestSendUnsubscribe()
    {
        var bus = new MessageBus();
        var sender = new Object();
        var payLoad = new Object();
        var listener = new TestListener();

        bus.Unsubscribe(bus.Subscribe(listener, 42));
        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(((System.Object)listener._message.sender) == null);
    }

    class ReactiveTestListener : IMessageListener
    {
        public Message _message = new Message();

        private IMessageBus _bus;
        public ReactiveTestListener(IMessageBus bus)
        {
            _bus= bus;
        }

        public void HandleMessage(Message message)
        {
            _bus.Send(_message.topic, _message.id, _message.sender, _message.payload);
        }
    }

    [Test]
    public void TestReactiveSend()
    {
        var bus = new MessageBus();
        var sender = new Object();
        var payLoad = new Object();
        var listener = new ReactiveTestListener(bus);

        listener._message.topic = 39;
        listener._message.id = 42;

        bus.Subscribe(listener, 42);
        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(bus.MessageCount == 1);
        Assert.IsTrue(bus.Read(0).topic == listener._message.topic);
        Assert.IsTrue(bus.Read(0).id == listener._message.id);
    }

}
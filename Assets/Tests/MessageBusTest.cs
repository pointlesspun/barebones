using System;
using BareBones.Common.Messages;
using NUnit.Framework;
using UnityEngine.TestTools;

public class MessageBusServiceTest
{
    // test utility class
    class ReactiveTestListener : IMessageListener
    {
        private Action<Message, IMessageBus> _reaction;

        private IMessageBus _bus;
        public ReactiveTestListener(IMessageBus bus, Action<Message, IMessageBus> reaction)
        {
            _bus = bus;
            _reaction = reaction;
        }

        public void HandleMessage(Message message)
        {
            _reaction(message, _bus);
        }
    }

    // trivial test if constructor works as intended
    [Test]
    public void ConstructorTest()
    {
        var bus = new MessageBus();

        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);
        Assert.IsTrue(bus.ListenerCount == 0);
    }

    // send a message see if it ends up in the queue
    [Test]
    public void TestSend()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();
       
        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);

        // sending while not updating so the message should end up in the read
        // queue right away
        bus.Send(42, 1, sender, payLoad);
        Assert.IsTrue(bus.MessageCount == 1);

        var message = bus.Read(0);
        Assert.IsTrue(((System.Object)message.sender) == sender);
        Assert.IsTrue(((System.Object)message.payload) == payLoad);
        Assert.IsTrue(message.topic == 42);
        Assert.IsTrue(message.id == 1);

        Assert.IsTrue(bus.Read(1) == null);

        bus.Update();

        Assert.IsTrue(bus.MessageCount == 0);
        Assert.IsTrue(bus.Read(0) == null);
    }

    // subscribe a listener see if it receives the message
    [Test]
    public void TestSendSubscribe()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();
        System.Object messageSender = default;
        var listener = new ReactiveTestListener(bus, (m, b) =>
        {
            messageSender = m.sender;
        });

        bus.Subscribe(listener, 42);
        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(messageSender == sender);
    }

    // subscribe then unsubscribe, see if the listener will no longer receive a message
    [Test]
    public void TestSendUnsubscribe()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();
        System.Object messageSender = default;
        var listener = new ReactiveTestListener(bus, (m, b) =>
        {
            messageSender = m.sender;
        });

        bus.Unsubscribe(bus.Subscribe(listener, 42));
        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(messageSender == default);
    }

    // receive a message, then in reply while the bus is updating send another
    // make sure the last send doesn't mess up the queue
    [Test]
    public void TestReactiveSend()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();
        var topic = 39;
        var id = 1;
        var listener = new ReactiveTestListener(bus, (m, b) =>
        {
            Assert.IsTrue(b.MessageCount == 1);

            b.Send(topic, id, sender, payLoad);

            // message should have gone in the write queue, so it's invisible to the message count
            Assert.IsTrue(b.MessageCount == 1);
        });

        Assert.IsTrue(!bus.Contains(listener));
        bus.Subscribe(listener, 42);
        Assert.IsTrue(bus.Contains(listener));

        bus.Send(42, 1, sender, payLoad);

        bus.Update();

        Assert.IsTrue(bus.MessageCount == 1);
        Assert.IsTrue(bus.Read(0).topic == topic);
        Assert.IsTrue(bus.Read(0).id == id);
    }

    // subscribe while the bus is updating, make sure this doesn't
    // mess up the listener enumeration
    [Test]
    public void TestReactiveSubscribe()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();

        System.Object messageSender = default;
        var captureListener = new ReactiveTestListener(bus, (m, b) =>
        {
            messageSender = m.sender;
        });

        var reactionListener = new ReactiveTestListener(bus, (m, b) =>
        {
            b.Subscribe(captureListener, 2);
        });

        bus.Subscribe(reactionListener, 1);

        Assert.IsTrue(!bus.Contains(captureListener));
        Assert.IsTrue(bus.ListenerCount == 1);

        bus.Send(1, 1, sender, payLoad);
        bus.Send(2, 1, sender, payLoad);
        bus.Update();

        Assert.IsTrue(bus.Contains(captureListener));
        Assert.IsTrue(bus.ListenerCount == 2);

        // capture listener should not have been activated yet and thus have
        // missed message with topic '2'
        Assert.IsTrue(messageSender == default);

        bus.Send(2, 1, sender, payLoad);
        bus.Update();

        // now it should have been activated 
        Assert.IsTrue(messageSender == sender);
    }

    // unsubscribe while the bus is updating, make sure this doesn't
    // mess up the listener enumeration
    [Test]
    public void TestReactiveUnsubscribe()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();

        var captureListenerHandle = -1;

        System.Object messageSender = default;
        var captureListener = new ReactiveTestListener(bus, (m, b) =>
        {
            messageSender = m.sender;
        });

        var reactionListener = new ReactiveTestListener(bus, (m, b) =>
        {
            b.Unsubscribe(captureListenerHandle);
        });

        bus.Subscribe(reactionListener, 1);
        captureListenerHandle = bus.Subscribe(captureListener, 2);

        Assert.IsTrue(bus.Contains(captureListener));
        Assert.IsTrue(bus.ListenerCount == 2);

        bus.Send(1, 1, sender, payLoad);
        bus.Update();

        bus.Send(2, 1, sender, payLoad);
        bus.Update();

        Assert.IsTrue(bus.Contains(reactionListener));
        Assert.IsTrue(!bus.Contains(captureListener));
        Assert.IsTrue(bus.ListenerCount == 1);

        // capture listener should not have received the message with topic 2 
        // becasue reactionListener unsubscibed it
        Assert.IsTrue(messageSender == default);
    }

    // a listener causing an exception during an update should not stop the bus from working
    [Test]
    public void ExceptionDuringUpdateTest()
    {
        var bus = new MessageBus();
        var sender = new System.Object();
        var payLoad = new System.Object();
        var wasExceptionThrown = false;

        System.Object messageSender = default;
        var captureListener = new ReactiveTestListener(bus, (m, b) =>
        {
            messageSender = m.sender;
        });

        var reactionListener = new ReactiveTestListener(bus, (m, b) =>
        {
            wasExceptionThrown = true;
            throw new Exception("fault");
        });
         
        // note that for this test we need to know in which order listeners
        // are registered. We need captureListener to be updated after reactionlistener
        bus.Subscribe(captureListener, 2);
        bus.Subscribe(reactionListener, 1);

        bus.Send(1, 1, sender, payLoad);
        bus.Send(2, 1, sender, payLoad);

        // ignore the errors causing the test to fail
        LogAssert.ignoreFailingMessages = true;
        bus.Update();
        LogAssert.ignoreFailingMessages = false;

        Assert.IsTrue(bus.Contains(reactionListener));
        Assert.IsTrue(bus.Contains(captureListener));
        Assert.IsTrue(bus.ListenerCount == 2);

        // the exception should have been caught & flagged but the messages should
        // still go on
        Assert.IsTrue(messageSender == sender);
        Assert.IsTrue(wasExceptionThrown);
    }

    // check if running out of messages works as intended and if free-ing up messages
    // makes them available again
    [Test]
    public void MessageLimitTest()
    {
        var bus = new MessageBus(1);

        Assert.IsTrue(bus.Send(0, 0, null, null));
        // no more messages available
        Assert.IsFalse(bus.Send(0, 0, null, null));

        bus.Update();

        // messages should be available again
        Assert.IsTrue(bus.Send(0, 0, null, null));
        Assert.IsFalse(bus.Send(0, 0, null, null));
    }

    // check if running out of listener slots works as intended and if free-ing up messages
    // makes them available again
    [Test]
    public void ListenerLimitTest()
    {
        var bus = new MessageBus(1,1);
        var handle = bus.Subscribe(new ReactiveTestListener(bus, null), 1);

        Assert.IsTrue(handle >= 0);
        Assert.IsTrue(bus.Subscribe(new ReactiveTestListener(bus, null), 1) < 0);

        bus.Unsubscribe(handle);
        Assert.IsTrue(bus.Subscribe(new ReactiveTestListener(bus, null), 1) >= 0);
    }
}
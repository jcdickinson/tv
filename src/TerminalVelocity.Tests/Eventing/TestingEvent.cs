using System;

namespace TerminalVelocity.Eventing
{
    public class TestingEvent : Event<TestingEventLoop, TestingEventData>
    {
        public TestingEvent(TestingEventLoop eventLoop) : base(eventLoop) { }

        public TestingEvent(EventSubscriber<TestingEventData> handler) : base(handler) { }

        public TestingEvent(Action<TestingEventData> handler) : base(handler) { }
    }

    public readonly struct TestingEventData
    {
        public readonly int Value;

        public TestingEventData(int value) => Value = value;
    }
}

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class BellEvent : Event<InteractionEventLoop, BellEventData>
    {
        public BellEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public BellEvent(EventSubscriber<BellEventData> handler) : base(handler) { }

        public BellEvent(Action<BellEventData> handler) : base(handler) { }
    }

    public readonly struct BellEventData
    {
        public override string ToString() => string.Empty;
    }
}

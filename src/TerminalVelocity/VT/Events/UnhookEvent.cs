using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class UnhookEvent : Event<InteractionEventLoop, UnhookEventData>
    {
        public UnhookEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public UnhookEvent(EventSubscriber<UnhookEventData> handler) : base(handler) { }

        public UnhookEvent(Action<UnhookEventData> handler) : base(handler) { }
    }

    public readonly struct UnhookEventData
    {
        public override string ToString() => string.Empty;
    }
}

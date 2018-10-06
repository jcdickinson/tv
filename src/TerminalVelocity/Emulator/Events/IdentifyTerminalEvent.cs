using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class IdentifyTerminalEvent : Event<InteractionEventLoop, IdentifyTerminalEventData>
    {
        public IdentifyTerminalEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public IdentifyTerminalEvent(EventSubscriber<IdentifyTerminalEventData> handler) : base(handler) { }

        public IdentifyTerminalEvent(Action<IdentifyTerminalEventData> handler) : base(handler) { }
    }

    public readonly struct IdentifyTerminalEventData
    {
        public override string ToString() => string.Empty;
    }
}

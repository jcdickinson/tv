using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class SetTabstopEvent : Event<InteractionEventLoop, SetTabstopEventData>
    {
        public SetTabstopEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public SetTabstopEvent(EventSubscriber<SetTabstopEventData> handler) : base(handler) { }

        public SetTabstopEvent(Action<SetTabstopEventData> handler) : base(handler) { }
    }

    public readonly struct SetTabstopEventData
    {
        public override string ToString() => string.Empty;
    }
}

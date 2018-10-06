using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class ResetColorEvent : Event<InteractionEventLoop, ResetColorEventData>
    {
        public ResetColorEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public ResetColorEvent(EventSubscriber<ResetColorEventData> handler) : base(handler) { }

        public ResetColorEvent(Action<ResetColorEventData> handler) : base(handler) { }
    }

    public readonly struct ResetColorEventData
    {
        public readonly NamedColor Index;

        public ResetColorEventData(NamedColor index)
            => Index = index;

        public override string ToString() => FormattableString.Invariant($"{Index}");
    }
}

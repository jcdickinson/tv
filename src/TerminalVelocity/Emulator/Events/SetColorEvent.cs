using System;
using System.Drawing;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class SetColorEvent : Event<InteractionEventLoop, SetColorEventData>
    {
        public SetColorEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public SetColorEvent(EventSubscriber<SetColorEventData> handler) : base(handler) { }

        public SetColorEvent(Action<SetColorEventData> handler) : base(handler) { }
    }

    public readonly struct SetColorEventData
    {
        public readonly NamedColor Index;

        public readonly Color Color;

        public SetColorEventData(NamedColor index, Color color)
            => (Index, Color) = (index, color);

        public override string ToString() => FormattableString.Invariant($"{Index}=({Color.R},{Color.G},{Color.B})");
    }
}

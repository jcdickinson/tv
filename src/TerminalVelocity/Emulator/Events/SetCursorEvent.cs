using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class SetCursorEvent : Event<InteractionEventLoop, SetCursorEventData>
    {
        public SetCursorEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public SetCursorEvent(EventSubscriber<SetCursorEventData> handler) : base(handler) { }

        public SetCursorEvent(Action<SetCursorEventData> handler) : base(handler) { }
    }

    public readonly struct SetCursorEventData
    {
        public readonly CursorStyle Style;

        public SetCursorEventData(CursorStyle style)
            => Style = style;

        public override string ToString() => FormattableString.Invariant($"{Style}");
    }
}

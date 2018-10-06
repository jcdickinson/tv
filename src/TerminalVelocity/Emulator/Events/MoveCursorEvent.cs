using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class MoveCursorEvent : Event<InteractionEventLoop, MoveCursorEventData>
    {
        public MoveCursorEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public MoveCursorEvent(EventSubscriber<MoveCursorEventData> handler) : base(handler) { }

        public MoveCursorEvent(Action<MoveCursorEventData> handler) : base(handler) { }
    }

    public readonly struct MoveCursorEventData
    {
        public readonly MoveOrigin Origin;
        public readonly MoveAxis Axis;
        public readonly int Count;

        public MoveCursorEventData(MoveOrigin origin, MoveAxis axis, int count)
            => (Origin, Axis, Count) = (origin, axis, count);

        public override string ToString() => FormattableString.Invariant($"{Origin} {Axis} {Count}");
    }
}

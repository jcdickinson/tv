using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class DeleteEvent : Event<InteractionEventLoop, DeleteEventData>
    {
        public DeleteEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public DeleteEvent(EventSubscriber<DeleteEventData> handler) : base(handler) { }

        public DeleteEvent(Action<DeleteEventData> handler) : base(handler) { }
    }

    public readonly struct DeleteEventData
    {
        public readonly DeleteDirection Direction;

        public DeleteEventData(DeleteDirection direction)
            => Direction = direction;

        public override string ToString() => FormattableString.Invariant($"{Direction}");
    }
}

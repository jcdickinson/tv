using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class StateEvent : Event<InteractionEventLoop, StateEventData>
    {
        public StateEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public StateEvent(EventSubscriber<StateEventData> handler) : base(handler) { }

        public StateEvent(Action<StateEventData> handler) : base(handler) { }
    }

    public readonly struct StateEventData
    {
        public readonly States States;

        public readonly StateMode Mode;

        public StateEventData(StateMode mode, States states)
            => (Mode, States) = (mode, states);

        public override string ToString() => FormattableString.Invariant($"{Mode} {States}");
    }
}

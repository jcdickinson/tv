using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class ExecuteEvent : Event<InteractionEventLoop, ExecuteEventData>
    {
        public ExecuteEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public ExecuteEvent(EventSubscriber<ExecuteEventData> handler) : base(handler) { }

        public ExecuteEvent(Action<ExecuteEventData> handler) : base(handler) { }
    }

    public readonly struct ExecuteEventData
    {
        public readonly ControlCode ControlCode;

        public ExecuteEventData(ControlCode controlCode) => ControlCode = controlCode;

        public override string ToString() => FormattableString.Invariant($"{ControlCode}");
    }
}

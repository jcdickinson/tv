using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Pty.Events
{
    [Event]
    public sealed class ReceiveEvent : Event<InteractionEventLoop, ReceiveEventData>
    {
        public ReceiveEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public ReceiveEvent(EventSubscriber<ReceiveEventData> handler) : base(handler) { }

        public ReceiveEvent(Action<ReceiveEventData> handler) : base(handler) { }
    }

    public readonly struct ReceiveEventData
    {
        public readonly ReadOnlyMemory<byte> Data;

        public ReceiveEventData(ReadOnlyMemory<byte> data) => Data = data;

        public override string ToString() => FormattableString.Invariant($"{Data.Length}");
    }
}

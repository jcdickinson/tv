using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class PutEvent : Event<InteractionEventLoop, PutEventData>
    {
        public PutEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public PutEvent(EventSubscriber<PutEventData> handler) : base(handler) { }

        public PutEvent(Action<PutEventData> handler) : base(handler) { }
    }

    public readonly struct PutEventData
    {
        public readonly byte Byte;

        public PutEventData(byte @byte) => Byte = @byte;

        public override string ToString() => ((char)Byte).ToString();
    }
}

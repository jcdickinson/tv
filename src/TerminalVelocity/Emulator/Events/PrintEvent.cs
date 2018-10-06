using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class PrintEvent : Event<InteractionEventLoop, PrintEventData>
    {
        public PrintEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public PrintEvent(EventSubscriber<PrintEventData> handler) : base(handler) { }

        public PrintEvent(Action<PrintEventData> handler) : base(handler) { }
    }

    public readonly struct PrintEventData
    {
        public readonly ReadOnlyMemory<char> Characters;

        public PrintEventData(ReadOnlyMemory<char> characters)
            => Characters = characters;

        public override string ToString() => new string(Characters.Span);
    }
}

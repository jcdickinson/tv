using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class WhitespaceEvent : Event<InteractionEventLoop, WhitespaceEventData>
    {
        public WhitespaceEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public WhitespaceEvent(EventSubscriber<WhitespaceEventData> handler) : base(handler) { }

        public WhitespaceEvent(Action<WhitespaceEventData> handler) : base(handler) { }
    }

    public readonly struct WhitespaceEventData
    {
        public readonly ReadOnlyMemory<char> Characters;

        public readonly int Count;

        public WhitespaceEventData(ReadOnlyMemory<char> characters, int count)
            => (Characters, Count) = (characters, count);

        public override string ToString()
        {
            var str = new string(Characters.Span);
            return FormattableString.Invariant($"{str}*{Count}");
        }
    }
}

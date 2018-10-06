using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class SetClipboardEvent : Event<InteractionEventLoop, SetClipboardEventData>
    {
        public SetClipboardEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public SetClipboardEvent(EventSubscriber<SetClipboardEventData> handler) : base(handler) { }

        public SetClipboardEvent(Action<SetClipboardEventData> handler) : base(handler) { }
    }

    public readonly struct SetClipboardEventData
    {
        public readonly ReadOnlyMemory<char> Characters;

        public SetClipboardEventData(ReadOnlyMemory<char> characters)
            => Characters = characters;

        public override string ToString() => new string(Characters.Span);
    }
}

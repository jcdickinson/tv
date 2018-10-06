using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Emulator.Events
{
    [Event]
    public sealed class SetWindowTitleEvent : Event<InteractionEventLoop, SetWindowTitleEventData>
    {
        public SetWindowTitleEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public SetWindowTitleEvent(EventSubscriber<SetWindowTitleEventData> handler) : base(handler) { }

        public SetWindowTitleEvent(Action<SetWindowTitleEventData> handler) : base(handler) { }
    }

    public readonly struct SetWindowTitleEventData
    {
        public readonly ReadOnlyMemory<char> Characters;

        public SetWindowTitleEventData(ReadOnlyMemory<char> characters) => Characters = characters;

        public override string ToString() => new string(Characters.Span);
    }
}

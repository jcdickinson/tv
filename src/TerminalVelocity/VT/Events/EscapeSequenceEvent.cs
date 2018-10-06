using System;
using System.Text;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class EscapeSequenceEvent : Event<InteractionEventLoop, EscapeSequenceEventData>
    {
        public EscapeSequenceEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public EscapeSequenceEvent(EventSubscriber<EscapeSequenceEventData> handler) : base(handler) { }

        public EscapeSequenceEvent(Action<EscapeSequenceEventData> handler) : base(handler) { }
    }

    public readonly struct EscapeSequenceEventData
    {
        public readonly EscapeCommand Command;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public EscapeSequenceEventData(
            EscapeCommand command,
            ReadOnlyMemory<byte> intermediates,
            IgnoredData ignored)
            => (Command, Intermediates, Ignored) = (command, intermediates, ignored);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Command);
            sb.Append("[");

            sb.Append(Encoding.ASCII.GetString(Intermediates.Span));
            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append("...");

            sb.Append("]");

            return sb.ToString();
        }
    }
}

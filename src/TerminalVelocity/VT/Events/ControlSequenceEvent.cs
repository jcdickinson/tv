using System;
using System.Text;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class ControlSequenceEvent : Event<InteractionEventLoop, ControlSequenceEventData>
    {
        public ControlSequenceEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public ControlSequenceEvent(EventSubscriber<ControlSequenceEventData> handler) : base(handler) { }

        public ControlSequenceEvent(Action<ControlSequenceEventData> handler) : base(handler) { }
    }

    public readonly struct ControlSequenceEventData
    {
        public readonly char Character;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public readonly ReadOnlyMemory<long> Parameters;

        public ControlSequenceEventData(
            ReadOnlyMemory<byte> intermediates,
            ReadOnlyMemory<long> parameters,
            IgnoredData ignored,
            char character)
            => (Intermediates, Parameters, Ignored, Character) = (intermediates, parameters, ignored, character);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Character).Append("(");

            for (var i = 0; i < Parameters.Length; i++)
            {
                sb.Append(i == 0 ? string.Empty : ";");
                sb.Append(Parameters.Span[i].ToString("x2"));
            }

            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append("...");

            sb.Append(")[");

            sb.Append(Encoding.ASCII.GetString(Intermediates.Span));
            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append("...");

            sb.Append("]");

            return sb.ToString();
        }
    }
}

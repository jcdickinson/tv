using System;
using System.Text;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.VT.Events
{
    [Event]
    public sealed class HookEvent : Event<InteractionEventLoop, HookEventData>
    {
        public HookEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }

        public HookEvent(EventSubscriber<HookEventData> handler) : base(handler) { }

        public HookEvent(Action<HookEventData> handler) : base(handler) { }
    }

    public readonly struct HookEventData
    {
        public readonly ReadOnlyMemory<long> Parameters;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public HookEventData(
            ReadOnlyMemory<long> parameters,
            ReadOnlyMemory<byte> intermediates,
            IgnoredData ignored)
            => (Parameters, Intermediates, Ignored) = (parameters, intermediates, ignored);

        public override string ToString()
        {
            var sb = new StringBuilder("(");

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

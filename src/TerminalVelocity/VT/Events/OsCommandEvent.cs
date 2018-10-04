using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct OsCommandEvent
    {
        public const string ContractName = "OSC.Events.VT.TerminalVelocity";

        public int Length => Parameters.Length;

        public ReadOnlySpan<byte> this[int index] => Parameters.Span[index].Span;

        public readonly ReadOnlyMemory<ReadOnlyMemory<byte>> Parameters;

        public readonly IgnoredData Ignored;

        public OsCommandEvent(
            ReadOnlyMemory<ReadOnlyMemory<byte>> parameters,
            IgnoredData ignored)
        {
            Parameters = parameters;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Parameters.Span.Length; i++)
            {
                sb.Append(i== 0 ? string.Empty : ";");
                sb.Append(Encoding.UTF8.GetString(Parameters.Span[i].Span));
            }

            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append("...");

            return sb.ToString();
        }
    }
}
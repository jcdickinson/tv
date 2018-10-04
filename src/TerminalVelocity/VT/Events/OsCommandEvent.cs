using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct OsCommandEvent
    {
        public const string ContractName = "OSC.Events.VT.TerminalVelocity";

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
            var sb = new StringBuilder("OSC");

            for (var i = 0; i < Parameters.Span.Length; i++)
            {
                sb.Append(i== 0 ? " " : "; ");

                var parameter = Parameters.Span[i].Span;
                for (var j = 0; j < parameter.Length; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(parameter[j].ToString("x2"));
                }
            }

            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append(Parameters.Length > 0 ? "; ignored" : " ignored");

            return sb.ToString();
        }
    }
}
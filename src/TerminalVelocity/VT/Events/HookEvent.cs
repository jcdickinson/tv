using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct HookEvent
    {
        public const string ContractName = "Hook.DCS.Events.VT.TerminalVelocity";

        public readonly ReadOnlyMemory<long> Parameters;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public HookEvent(
            ReadOnlyMemory<long> parameters, 
            ReadOnlyMemory<byte> intermediates,
            IgnoredData ignored)
        { 
            Parameters = parameters;
            Intermediates = intermediates;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("DCS Hook (");

            for (var i = 0; i < Parameters.Length; i++)
            {
                sb.Append(i == 0 ? string.Empty : "; ");
                sb.Append(Parameters.Span[i].ToString("x2"));
            }
            
            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append(Parameters.Length > 0 ? "; ignored" : "ignored");

            sb.Append(")");

            for (var i = 0; i < Intermediates.Length; i++)
            {
                sb.Append(i == 0 ? " " : "; ");
                sb.Append(Intermediates.Span[i].ToString("x2"));
            }

            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append(Intermediates.Length > 0 ? "; ignored" : " ignored");

            return sb.ToString();
        }
    }
}
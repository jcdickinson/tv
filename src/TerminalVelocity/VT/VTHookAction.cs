using System;
using System.Globalization;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTHookAction
    {
        public ReadOnlySpan<long> Parameters { get; }

        public ReadOnlySpan<byte> Intermediates { get; }

        public bool IntermediatesIgnored { get; }

        public VTHookAction(ReadOnlySpan<long> parameters, ReadOnlySpan<byte> intermediates, bool ignored)
        { 
            Parameters = parameters;
            Intermediates = intermediates;
            IntermediatesIgnored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("Hook (");

            for (var i = 0; i < Parameters.Length; i++)
            {
                sb.Append(i == 0 ? string.Empty : "; ");
                sb.Append(Parameters[i].ToString("x2"));
            }

            sb.Append(")");

            for (var i = 0; i < Intermediates.Length; i++)
            {
                sb.Append(i == 0 ? " " : "; ");
                sb.Append(Intermediates[i].ToString("x2"));
            }

            if (IntermediatesIgnored)
                sb.Append(Intermediates.Length > 0 ? "; ignored" : " ignored");

            return sb.ToString();
        }
    }
}
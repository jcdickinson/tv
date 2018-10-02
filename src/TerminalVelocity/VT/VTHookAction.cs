using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTHookAction
    {
        public ReadOnlySpan<long> Parameters { get; }

        public ReadOnlySpan<byte> Intermediates { get; }

        public VTIgnore Ignored { get; }

        public VTHookAction(ReadOnlySpan<long> parameters, ReadOnlySpan<byte> intermediates, VTIgnore ignored)
        { 
            Parameters = parameters;
            Intermediates = intermediates;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("Hook (");

            for (var i = 0; i < Parameters.Length; i++)
            {
                sb.Append(i == 0 ? string.Empty : "; ");
                sb.Append(Parameters[i].ToString("x2"));
            }
            
            if (Ignored.HasFlag(VTIgnore.Parameters))
                sb.Append(Parameters.Length > 0 ? "; ignored" : "ignored");

            sb.Append(")");

            for (var i = 0; i < Intermediates.Length; i++)
            {
                sb.Append(i == 0 ? " " : "; ");
                sb.Append(Intermediates[i].ToString("x2"));
            }

            if (Ignored.HasFlag(VTIgnore.Intermediates))
                sb.Append(Intermediates.Length > 0 ? "; ignored" : " ignored");

            return sb.ToString();
        }
    }
}
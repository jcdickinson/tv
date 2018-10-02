using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTEscDispatchAction
    {
        public byte Byte { get; }

        public ReadOnlySpan<byte> Intermediates { get; }

        public ReadOnlySpan<long> Parameters { get; }

        public bool IntermediatesIgnored { get; }

        public VTEscDispatchAction(
            ReadOnlySpan<byte> intermediates,
            ReadOnlySpan<long> parameters,
            bool ignored,
            byte @byte)
        {
            Byte = @byte;
            Intermediates = intermediates;
            Parameters = parameters;
            IntermediatesIgnored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("ESC Dispatch ");

            sb.Append(((int)Byte).ToString("x2"))
                .Append(" (");

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
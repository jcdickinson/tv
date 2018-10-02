using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTCsiDispatchAction
    {
        public char Character { get; }

        public ReadOnlySpan<byte> Intermediates { get; }

        public VTIgnore Ignored { get; }

        public ReadOnlySpan<long> Parameters { get; }

        public VTCsiDispatchAction(
            ReadOnlySpan<byte> intermediates,
            ReadOnlySpan<long> parameters,
            VTIgnore ignored,
            char character)
        {
            Character = character;
            Intermediates = intermediates;
            Parameters = parameters;
            Ignored = ignored;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("CSI Dispatch ");

            sb.Append(((int)Character).ToString("x2"))
                .Append(" '")
                .Append(Character)
                .Append("' (");

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
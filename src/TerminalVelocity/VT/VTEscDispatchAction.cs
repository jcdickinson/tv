using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTEscDispatchAction
    {
        public byte Byte { get; }

        public ReadOnlySpan<byte> Intermediates { get; }

        public VTIgnore Ignored { get; }

        public VTEscDispatchAction(
            ReadOnlySpan<byte> intermediates,
            VTIgnore ignored,
            byte @byte)
        {
            Byte = @byte;
            Intermediates = intermediates;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("ESC Dispatch ");

            sb.Append(((int)Byte).ToString("x2"));

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
using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct EscapeSequenceEvent
    {
        public const string ContractName = "ESC.Events.VT.TerminalVelocity";

        public readonly byte Byte;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public EscapeSequenceEvent(
            ReadOnlyMemory<byte> intermediates,
            IgnoredData ignored,
            byte @byte)
        {
            Byte = @byte;
            Intermediates = intermediates;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("ESC ");

            sb.Append(((int)Byte).ToString("x2"));

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
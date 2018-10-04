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
            var sb = new StringBuilder();

            sb.Append((char)Byte);
            sb.Append(";");

            sb.Append(Encoding.ASCII.GetString(Intermediates.Span));
            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append("...");

            return sb.ToString();
        }
    }
}
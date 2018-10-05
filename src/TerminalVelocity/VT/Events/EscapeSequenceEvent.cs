using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct EscapeSequenceEvent
    {
        public const string ContractName = "ESC.Events.VT.TerminalVelocity";

        public readonly EscapeCommand Command;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public EscapeSequenceEvent(
            EscapeCommand command,
            ReadOnlyMemory<byte> intermediates,
            IgnoredData ignored)
        {
            Command = command;
            Intermediates = intermediates;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Command);
            sb.Append("[");

            sb.Append(Encoding.ASCII.GetString(Intermediates.Span));
            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append("...");
            
            sb.Append("]");

            return sb.ToString();
        }
    }
}
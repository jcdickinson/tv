using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct ControlSequenceEvent
    {
        public const string ContractName = "CSI.Events.VT.TerminalVelocity";

        public readonly char Character;

        public readonly ReadOnlyMemory<byte> Intermediates;

        public readonly IgnoredData Ignored;

        public readonly ReadOnlyMemory<long> Parameters;

        public ControlSequenceEvent(
            ReadOnlyMemory<byte> intermediates,
            ReadOnlyMemory<long> parameters,
            IgnoredData ignored,
            char character)
        {
            Character = character;
            Intermediates = intermediates;
            Parameters = parameters;
            Ignored = ignored;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Character).Append("(");

            for (var i = 0; i < Parameters.Length; i++)
            {
                sb.Append(i == 0 ? string.Empty : ";");
                sb.Append(Parameters.Span[i].ToString("x2"));
            }
            
            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append("...");

            sb.Append(")");

            sb.Append(Encoding.ASCII.GetString(Intermediates.Span));
            if (Ignored.HasFlag(IgnoredData.Intermediates))
                sb.Append("...");

            return sb.ToString();
        }
    }
}
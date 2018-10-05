using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct OsCommandEvent
    {
        public const string ContractName = "OSC.Events.VT.TerminalVelocity";

        public int Length => _parameters.Length;

        public ReadOnlySpan<byte> this[int index] => _parameters.Span[index].Span;

        public readonly OsCommand Command;

        private readonly ReadOnlyMemory<ReadOnlyMemory<byte>> _parameters;

        public readonly IgnoredData Ignored;

        public OsCommandEvent(
            ReadOnlyMemory<ReadOnlyMemory<byte>> parameters,
            IgnoredData ignored)
        {
            if (parameters.Length == 0 ||
                !TryParseInt16(parameters.Span[0].Span, out var cmd))
            {
                Command = OsCommand.Unknown;
                _parameters = parameters;
            }
            else
            {
                Command = (OsCommand)cmd;
                _parameters = parameters.Slice(1);
            }
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Command);
            sb.Append("(");

            for (var i = 0; i < _parameters.Span.Length; i++)
            {
                sb.Append(i== 0 ? string.Empty : ";");
                sb.Append(Encoding.UTF8.GetString(_parameters.Span[i].Span));
            }

            if (Ignored.HasFlag(IgnoredData.Parameters))
                sb.Append("...");
            sb.Append(")");

            return sb.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseInt16(ReadOnlySpan<byte> raw, out short result)
        {
            result = 0;
            if (raw.Length == 0) return false;

            for (var i = 0; i < raw.Length; i++)
            {
                result = (short)((result * 10) + (raw[i] - (byte)'0'));
                if (result < 0)
                {
                    result = 0;
                    return false;
                }
            }
            return true;
        }
    }
}
using System;
using System.Text;

namespace TerminalVelocity.Pty.Events
{
    public readonly struct ReceiveEvent
    {
        public const string ContractName = "Receive.Events.Pty.TerminalVelocity";

        public readonly ReadOnlyMemory<byte> Data;

        public ReceiveEvent(ReadOnlyMemory<byte> data)
        {
            Data = data;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Data.Length}");
    }
}
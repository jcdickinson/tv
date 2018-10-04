using System;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct DeleteEvent
    {
        public const string ContractName = "Delete.Events.Emulator.TerminalVelocity";

        public readonly Delete Direction;

        public DeleteEvent(Delete direction)
        {
            Direction = direction;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Direction}");
    }
}
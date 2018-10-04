using System;
using System.Drawing;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct ResetColorEvent
    {
        public const string ContractName = "ResetColor.Events.Emulator.TerminalVelocity";

        public readonly NamedColor Index;

        public ResetColorEvent(NamedColor index)
        {
            Index = index;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Index}");
    }
}
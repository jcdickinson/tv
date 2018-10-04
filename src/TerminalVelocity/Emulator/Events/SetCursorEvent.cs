using System;
using System.Drawing;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct SetCursorEvent
    {
        public const string ContractName = "SetCursor.Events.Emulator.TerminalVelocity";

        public readonly CursorStyle Style;

        public SetCursorEvent(CursorStyle style)
        {
            Style = style;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Style}");
    }
}
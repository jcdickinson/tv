using System;
using System.Drawing;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct SetColorEvent
    {
        public const string ContractName = "SetColor.Events.Emulator.TerminalVelocity";

        public readonly NamedColor Index;

        public readonly Color Color;

        public SetColorEvent(NamedColor index, Color color)
        {
            Index = index;
            Color = color;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Index}=({Color.R},{Color.G},{Color.B})");
    }
}
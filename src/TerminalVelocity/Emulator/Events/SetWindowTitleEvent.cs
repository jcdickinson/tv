using System;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct SetWindowTitleEvent
    {
        public const string ContractName = "SetWindowTitle.Events.Emulator.TerminalVelocity";

        public readonly ReadOnlyMemory<char> Characters;

        public SetWindowTitleEvent(ReadOnlyMemory<char> characters)
        {
            Characters = characters;
        }
        
        public override string ToString() => new string(Characters.Span);
    }
}
using System;
using System.Text;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct SetClipboardEvent
    {
        public const string ContractName = "SetClipboard.Events.Emulator.TerminalVelocity";

        public readonly ReadOnlyMemory<char> Characters;

        public SetClipboardEvent(ReadOnlyMemory<char> characters)
        {
            Characters = characters;
        }
        
        public override string ToString() => new string(Characters.Span);
    }
}
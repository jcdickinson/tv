using System;
using System.Text;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct PrintEvent
    {
        public const string ContractName = "Print.Events.Emulator.TerminalVelocity";

        public readonly ReadOnlyMemory<char> Characters;

        public PrintEvent(ReadOnlyMemory<char> characters)
        {
            Characters = characters;
        }
        
        public override string ToString() => new string(Characters.Span);
    }
}
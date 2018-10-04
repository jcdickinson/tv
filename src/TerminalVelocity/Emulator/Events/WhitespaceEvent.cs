using System;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct WhitespaceEvent
    {
        public const string ContractName = "Whitespace.Events.Emulator.TerminalVelocity";

        public readonly ReadOnlyMemory<char> Characters;

        public readonly int Count;

        public WhitespaceEvent(ReadOnlyMemory<char> characters, int count)
        {
            Characters = characters;
            Count = count;
        }
        
        public override string ToString()
        {
            var str = new string(Characters.Span);
            return FormattableString.Invariant($"{str}*{Count}");
        } 
    }
}
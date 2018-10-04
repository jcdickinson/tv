using System;
using System.Text;

namespace TerminalVelocity.VT.Events
{
    public readonly struct PrintEvent
    {
        public const string ContractName = "Print.Events.VT.TerminalVelocity";

        public readonly ReadOnlyMemory<char> Characters;

        public PrintEvent(ReadOnlyMemory<char> characters)
        {
            Characters = characters;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("Print ");
            sb.Append(new string(Characters.Span));
            return sb.ToString();
        }
    }
}
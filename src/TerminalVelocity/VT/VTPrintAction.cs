using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTPrintAction
    {
        public ReadOnlySpan<char> Characters { get; }

        public VTPrintAction(ReadOnlySpan<char> characters)
        {
            Characters = characters;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("Print ");
            sb.Append(new string(Characters));
            return sb.ToString();
        }
    }
}
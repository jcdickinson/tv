using System.Composition;
using System.Drawing;

namespace TerminalVelocity.Preferences
{
    [Shared]
    public class TerminalTheme
    {
        public string Font { get; }
        public Color TerminalBackground { get; }
        public Color Color1 { get; }

        public TerminalTheme()
        {
            Font = "Fira Code";
            TerminalBackground = Color.Black;
            Color1 = Color.White;
        }
    }
}
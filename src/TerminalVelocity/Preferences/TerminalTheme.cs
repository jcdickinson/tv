using System.Composition;
using System.Drawing;

namespace TerminalVelocity.Preferences
{
    [Shared]
    public class TerminalTheme
    {
        public const string FontContract = "Font.Terminal.TerminalVelocity";
        public const string Color0Contract = "0.Color.Terminal.TerminalVelocity";
        public const string Color1Contract = "1.Color.Terminal.TerminalVelocity";

        [Export(FontContract)]
        public Configurable<string> Font { get; }
        [Export(FontContract)]
        public Configurable<int> FontSize { get; }
        [Export(Color0Contract)]
        public Configurable<Color> Color0 { get; }
        [Export(Color1Contract)]
        public Configurable<Color> Color1 { get; }

        public TerminalTheme()
        {
            Font = "Fira Code";
            Color0 = Color.Black;
            Color1 = Color.White;
            FontSize = 16;
        }
    }
}
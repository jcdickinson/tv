using System.Drawing;

namespace TerminalVelocity.Preferences
{
    public class TerminalConfiguration
    {
        public Configurable<string> Font { get; }
        public Configurable<int> FontSize { get; }
        public Configurable<Color> Color0 { get; }
        public Configurable<Color> Color1 { get; }

        public TerminalConfiguration()
        {
            Font = "Fira Code";
            Color0 = Color.Black;
            Color1 = Color.White;
            FontSize = 16;
        }
    }
}

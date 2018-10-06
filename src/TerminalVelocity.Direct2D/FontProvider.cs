using System.Drawing;
using SharpDX.DirectWrite;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D
{
    public class FontProvider
    {
        public Configurable<TextFormat> TerminalText { get; }

        private readonly Factory _factory;

        public FontProvider(
            Factory factory,
            TerminalConfiguration terminalConfiguration
        )
        {
            _factory = factory;
            TerminalText = terminalConfiguration.Font.Join(terminalConfiguration.FontSize, TextFormat);
        }

        private TextFormat TextFormat(string font, Size fontSize) => TextFormat(font, fontSize.Height);

        private TextFormat TextFormat(string font, int fontSize) =>
            new TextFormat(_factory, font, fontSize);
    }
}

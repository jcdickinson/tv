using System.Composition;
using System.Drawing;
using SharpDX;
using SharpDX.DirectWrite;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public class FontProvider
    {
        public const string CaptionTextContract = WindowsMetricsProvider.CaptionTextContract;

        // TODO: SVG
        [Export(CaptionTextContract)]
        public Configurable<TextFormat> CaptionTextTextFormat { get; }

        private Factory _factory;

        [ImportingConstructor]
        public FontProvider(
            [Import] Factory factory,
            [Import(WindowsMetricsProvider.CaptionTextContract)] Configurable<string> captionTextFamily,
            [Import(WindowsMetricsProvider.CaptionTextContract)] Configurable<Size> captionTextSize
        )
        {
            _factory = factory;
            CaptionTextTextFormat = captionTextFamily.Join(captionTextSize, TextFormat);
        }

        private TextFormat TextFormat(string font, Size fontSize) =>
            new TextFormat(_factory, font, fontSize.Height);
    }
}
using System;
using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public class BrushProvider
    {
        public const string ChromeBackgroundContract = "ChromeBackground.UI.Direct2D.TerminalVelocity";
        public const string ChromeTextContract = "ChromeText.UI.Direct2D.TerminalVelocity";
        public const string ChromeButtonContract = "ChromeButton.UI.Direct2D.TerminalVelocity";
        public const string ChromeMinButtonContract = "ChromeMinButton.UI.Direct2D.TerminalVelocity";
        public const string ChromeMaxButtonContract = "ChromeMaxButton.UI.Direct2D.TerminalVelocity";
        public const string ChromeRestoreButtonContract = "ChromeRestoreButton.UI.Direct2D.TerminalVelocity";
        public const string ChromeCloseButtonContract = "ChromeCloseButton.UI.Direct2D.TerminalVelocity";
        public const string LogoContract = "Logo.UI.Direct2D.TerminalVelocity";

        public const string TerminalColor0Contract = TerminalTheme.Color0Contract;
        public const string TerminalColor1Contract = TerminalTheme.Color1Contract;

        [Export(ChromeBackgroundContract)]
        public Configurable<Brush> ChromeBackground { get; }
        [Export(ChromeTextContract)]
        public Configurable<Brush> ChromeText { get; }
        [Export(ChromeButtonContract)]
        public Configurable<Brush> ChromeButton { get; }
        [Export(ChromeMinButtonContract)]
        public Configurable<Brush> ChromeMinButton { get; }
        [Export(ChromeMaxButtonContract)]
        public Configurable<Brush> ChromeMaxButton { get; }
        [Export(ChromeRestoreButtonContract)]
        public Configurable<Brush> ChromeRestoreButton { get; }
        [Export(ChromeCloseButtonContract)]
        public Configurable<Brush> ChromeCloseButton { get; }
        [Export(LogoContract)]
        public Configurable<Brush> Logo { get; }
        [Export(TerminalColor0Contract)]
        public Configurable<Brush> TerminalColor0 { get; }
        [Export(TerminalColor1Contract)]
        public Configurable<Brush> TerminalColor1 { get; }

        private readonly DeviceContext _deviceContext;

        [ImportingConstructor]
        public BrushProvider(
            [Import(WindowTheme.ChromeBackgroundContract)] Configurable<System.Drawing.Color> chromeBackground,
            [Import(WindowTheme.ChromeTextContract)] Configurable<System.Drawing.Color> chromeText,
            [Import(WindowTheme.ChromeButtonContract)] Configurable<System.Drawing.Color> chromeButton,
            [Import(WindowTheme.ChromeMinButtonContract)] Configurable<System.Drawing.Color> chromeMinButton,
            [Import(WindowTheme.ChromeMaxButtonContract)] Configurable<System.Drawing.Color> chromeMaxButton,
            [Import(WindowTheme.ChromeRestoreButtonContract)] Configurable<System.Drawing.Color> chromeRestoreButton,
            [Import(WindowTheme.ChromeCloseButtonContract)] Configurable<System.Drawing.Color> chromeCloseButton,
            [Import(WindowTheme.LogoContract)] Configurable<System.Drawing.Color> logo,
            [Import(TerminalTheme.Color0Contract)] Configurable<System.Drawing.Color> terminalColor0,
            [Import(TerminalTheme.Color1Contract)] Configurable<System.Drawing.Color> terminalColor1,
            [Import] DeviceContext deviceContext)
        {
            _deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext));

            ChromeBackground = chromeBackground.Select(SolidColorBrush);
            ChromeText = chromeText.Select(SolidColorBrush);
            ChromeButton = chromeButton.Select(SolidColorBrush);
            ChromeMinButton = chromeMinButton.Select(SolidColorBrush);
            ChromeMaxButton = chromeMaxButton.Select(SolidColorBrush);
            ChromeRestoreButton = chromeRestoreButton.Select(SolidColorBrush);
            ChromeCloseButton = chromeCloseButton.Select(SolidColorBrush);
            Logo = logo.Select(SolidColorBrush);
            TerminalColor0 = terminalColor0.Select(SolidColorBrush);
            TerminalColor1 = terminalColor1.Select(SolidColorBrush);
        }

        private Brush SolidColorBrush(System.Drawing.Color color)
            => new SolidColorBrush(_deviceContext, color.ToSharpDX());
    }
}
using System.Composition;
using System.Drawing;

namespace TerminalVelocity.Preferences
{
    [Shared]
    public class WindowTheme
    {
        public const string ChromeBackgroundContract = "ChromeBackground.UI.TerminalVelocity";
        public const string ChromeTextContract = "ChromeText.UI.TerminalVelocity";
        public const string ChromeButtonContract = "ChromeButton.UI.TerminalVelocity";
        public const string ChromeMinButtonContract = "ChromeMinButton.UI.TerminalVelocity";
        public const string ChromeMaxButtonContract = "ChromeMaxButton.UI.TerminalVelocity";
        public const string ChromeRestoreButtonContract = "ChromeRestoreButton.UI.TerminalVelocity";
        public const string ChromeCloseButtonContract = "ChromeCloseButton.UI.TerminalVelocity";
        public const string LogoContract = "Logo.UI.TerminalVelocity";

        [Export(ChromeBackgroundContract)]
        public Configurable<Color> ChromeBackground { get; }
        [Export(ChromeTextContract)]
        public Configurable<Color> ChromeText { get; }
        [Export(ChromeButtonContract)]
        [Export(ChromeMinButtonContract)]
        [Export(ChromeRestoreButtonContract)]
        [Export(ChromeMaxButtonContract)]
        public Configurable<Color> ChromeButton { get; }
        [Export(ChromeCloseButtonContract)]
        public Configurable<Color> ChromeCloseButton { get; }
        [Export(LogoContract)]
        public Configurable<Color> Logo { get; }

        public WindowTheme()
        {
            ChromeBackground = Color.FromArgb(70, 70, 70);
            ChromeText = Color.White;
            ChromeButton = Color.FromArgb(70, Color.White);
            ChromeCloseButton = Color.Red;
            Logo = Color.FromArgb(0, 204, 79);
        }
    }
}
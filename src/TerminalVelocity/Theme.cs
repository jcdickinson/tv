using SharpDX;
using SharpDX.Mathematics.Interop;

namespace TerminalVelocity
{
    public class Theme
    {
        public Color4 ChromeBackground { get; }
        public Color4 ChromeText { get; }
        public Color4 ChromeButtonHover { get; }
        public Color4 ChromeCloseButtonHover { get; }
        public Color4 Logo { get; }

        public string Font { get; }
        public Color4 TerminalBackground { get; }
        public Color4 Color1 { get; }

        public Theme()
        {
            ChromeBackground = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
            ChromeText = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            ChromeButtonHover = new Color4(1.0f, 1.0f, 1.0f, 0.4f);
            ChromeCloseButtonHover = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            Logo = new Color4(0.0f, 0.8f, 0.3f, 1.0f);

            TerminalBackground = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            Color1 = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            Font = "Fira Code";
        }
    }
}
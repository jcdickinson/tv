using SharpDX.Mathematics.Interop;

namespace TerminalVelocity
{
    public class Theme
    {
        public RawColor4 Window { get; }

        public Theme()
        {
            Window = new RawColor4(0.1f, 0.1f, 0.1f, 1.0f);
        }
    }
}
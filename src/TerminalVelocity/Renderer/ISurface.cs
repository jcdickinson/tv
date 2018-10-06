using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface ISurface
    {
        IFont CreateFont(string family, float size);
        ISolidColorBrush CreateSolidColorBrush(in Color color);
        void FillRectangle(IBrush brush, in RectangleF rectangle);
    }
}

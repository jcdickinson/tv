using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface ISolidColorBrush : IBrush
    {
        Color Color { get; set; }
    }
}

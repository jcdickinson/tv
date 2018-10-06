using System;
using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface IText : IDisposable
    {
        IFont DefaultFont { get; }
        ReadOnlyMemory<char> Text { get; }
        SizeF Layout { get; }
        IBrush DefaultBrush { get; }

        void SetBrush(TextRange range, IBrush brush);
        void SetFont(TextRange range, IFont font);
        TextMetrics CalculateMetrics();

        void Draw(PointF location);
    }
}

using System;
using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface IFont : IDisposable
    {
        string Family { get; }
        float Size { get; }

        IText CreateText(IBrush defaultBrush, ReadOnlyMemory<char> text, SizeF layout);
    }
}

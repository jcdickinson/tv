using System;
using SharpDX.Mathematics.Interop;

namespace TerminalVelocity.Direct2D
{
    public static class GeometryExtensions
    {
        public static bool Contains(this RawRectangleF rect, RawPoint point)
            => point.X >= rect.Left && point.X <= rect.Right
            && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }
}
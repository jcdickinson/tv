using System;
using SharpDX;

namespace TerminalVelocity.Direct2D.UI
{
    internal static class RectangleFUtils
    {
        public static RectangleF Rect(float left, float top, float right, float bottom) => new RectangleF(
            left, top,
            right - left, bottom - top
        );
    }
}
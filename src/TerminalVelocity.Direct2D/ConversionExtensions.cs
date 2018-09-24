using System;
using System.Drawing;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace TerminalVelocity.Direct2D
{
    public static class ConversionExtensions
    {
        private const float MaxColorComponent = 255f;

        internal static Color4 ToSharpDX(this System.Drawing.Color color) =>
            new Color4(color.R / MaxColorComponent, color.G / MaxColorComponent, color.B / MaxColorComponent, color.A / MaxColorComponent);
    }
}
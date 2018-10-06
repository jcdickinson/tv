using System;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private const float MaxColorComponent = 255f;

        private Size2F _dpiScale;

        public static void Convert(in System.Drawing.Color color, out RawColor4 color4) 
            => color4 = new RawColor4(
                color.R / MaxColorComponent, 
                color.G / MaxColorComponent, 
                color.B / MaxColorComponent, 
                color.A / MaxColorComponent);

        public static void Convert(in RawColor4 color4, out System.Drawing.Color color)
            => color = System.Drawing.Color.FromArgb(
                (int)MathUtil.Clamp(color4.R * MaxColorComponent, 0, 255),
                (int)MathUtil.Clamp(color4.G * MaxColorComponent, 0, 255),
                (int)MathUtil.Clamp(color4.B * MaxColorComponent, 0, 255),
                (int)MathUtil.Clamp(color4.A * MaxColorComponent, 0, 255));

        public static void Convert(in System.Drawing.PointF point, out RawVector2 vector2)
            => vector2 = new RawVector2(point.X, point.Y);

        public static void Convert(in Renderer.TextRange textRange, out TextRange result)
            => result = new TextRange(textRange.Offset, textRange.Length);

        public static void Convert(in TextMetrics metrics, out Renderer.TextMetrics result)
            =>  result = new Renderer.TextMetrics(
                left: metrics.Left,
                top: metrics.Top,
                width: metrics.Width,
                widthIncludingTrailingWhitespace: metrics.WidthIncludingTrailingWhitespace,
                height: metrics.Height,
                layoutWidth: metrics.LayoutWidth,
                layoutHeight: metrics.LayoutHeight,
                maxBidiReorderingDepth: metrics.MaxBidiReorderingDepth,
                lineCount: metrics.LineCount);

        public void ConvertToPixels(in System.Drawing.SizeF size, out Size2 size2)
        {
            Size2F dpiScale = _dpiScale;
            size2 = new Size2(
                (int)(size.Width * dpiScale.Width),
                (int)(size.Height * dpiScale.Height));
        }

        public void ConvertToPixels(in System.Drawing.RectangleF rectangleF, out RawRectangleF rawRectangleF)
        {
            Size2F dpiScale = _dpiScale;
            rawRectangleF = new RawRectangleF(
                (int)(rectangleF.Left * dpiScale.Width),
                (int)(rectangleF.Top * dpiScale.Height),
                (int)(rectangleF.Width * dpiScale.Width),
                (int)(rectangleF.Height * dpiScale.Height));
        }
    }
}

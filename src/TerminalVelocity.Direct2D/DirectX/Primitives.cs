using System;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Renderer;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface : ISurface
    {
        private const string Error_FactoryObject = "The value must be created by this ISurface.";

        private sealed class DxFont : IFont
        {
            public readonly Surface Surface;
            public TextFormat TextFormat;

            public string Family => TextFormat.FontFamilyName;
            public float Size => TextFormat.FontSize;

            public DxFont(Surface surface, TextFormat textFormat)
            {
                Surface = surface;
                TextFormat = textFormat;
            }

            public IText CreateText(IBrush defaultBrush, ReadOnlyMemory<char> text, SizeF layout)
            {
                if (!(defaultBrush is IDxBrush dxBrush)) throw new ArgumentOutOfRangeException(nameof(defaultBrush), Error_FactoryObject);
                if (layout.Width < 0 || layout.Height < 0) throw new ArgumentOutOfRangeException(nameof(layout));

                var str = new string(text.Span);

                TextLayout tlayout = Surface.IsDisposing
                    ? null
                    : new TextLayout(Surface._write.Factory, str, TextFormat, layout.Width, layout.Height);
                return new DxText(Surface, text, tlayout, this, dxBrush);
            }

            public void Dispose() => Disposable.Dispose(ref TextFormat);
        }

        private sealed class DxText : IText
        {
            public readonly Surface Surface;
            public readonly IDxBrush DefaultBrush;
            public TextLayout TextLayout;

            public IFont DefaultFont { get; }
            IBrush IText.DefaultBrush => DefaultBrush;
            public SizeF Layout => new SizeF(TextLayout.MaxWidth, TextLayout.MaxHeight);
            public ReadOnlyMemory<char> Text { get; }

            public DxText(Surface surface, ReadOnlyMemory<char> text, TextLayout textLayout, DxFont defaultFont, IDxBrush defaultBrush)
            {
                Surface = surface;
                TextLayout = textLayout;

                DefaultFont = defaultFont;
                DefaultBrush = defaultBrush;
                Text = text;
            }

            public void Dispose() => Disposable.Dispose(ref TextLayout);

            public void SetBrush(Renderer.TextRange range, IBrush brush)
            {
                if (Surface.IsDisposing) return;

                if (!(brush is IDxBrush dxBrush)) throw new ArgumentOutOfRangeException(nameof(brush), Error_FactoryObject);
                Convert(range, out SharpDX.DirectWrite.TextRange dxRange);
                TextLayout.SetDrawingEffect(dxBrush.Brush, dxRange);
            }

            public void SetFont(Renderer.TextRange range, IFont font)
            {
                if (Surface.IsDisposing) return;

                if (!(font is DxFont dxFont)) throw new ArgumentOutOfRangeException(nameof(font), Error_FactoryObject);
                Convert(range, out SharpDX.DirectWrite.TextRange dxRange);
                TextLayout.SetFontFamilyName(dxFont.Family, dxRange);
                TextLayout.SetFontSize(dxFont.Size, dxRange);
            }

            public Renderer.TextMetrics CalculateMetrics()
            {
                if (Surface.IsDisposing) return default;

                Convert(TextLayout.Metrics, out Renderer.TextMetrics metrics);
                return metrics;
            }

            public void Draw(PointF location)
            {
                if (!Surface.IsDrawing) return;

                Convert(location, out RawVector2 vector);
                Surface._d2d.Context.DrawTextLayout(vector, TextLayout, DefaultBrush.Brush, DrawTextOptions.EnableColorFont);
            }
        }

        private interface IDxBrush : IBrush
        {
            Brush Brush { get; }
        }

        private sealed class DxSolidColorBrush : IDxBrush, ISolidColorBrush
        {
            public readonly Surface Surface;
            public SolidColorBrush Brush;

            Brush IDxBrush.Brush => Brush;

            public Color Color
            {
                get
                {
                    if (Surface.IsDisposing) return default;

                    Convert(Brush.Color, out Color color);
                    return color;
                }
                set
                {
                    if (Surface.IsDisposing) return;

                    Convert(value, out RawColor4 color4);
                    Brush.Color = color4;
                }
            }

            public DxSolidColorBrush(Surface surface, SolidColorBrush brush)
            {
                Surface = surface;
                Brush = brush;
            }

            public void Dispose() => Disposable.Dispose(ref Brush);
        }

        public IFont CreateFont(string family, float size)
        {
            CheckThread();

            if (string.IsNullOrEmpty(family)) throw new ArgumentNullException(nameof(family));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            TextFormat font = IsDisposing
                ? null
                : new TextFormat(_write.Factory, family, size);
            return new DxFont(this, font);
        }

        public ISolidColorBrush CreateSolidColorBrush(in Color color)
        {
            CheckThread();

            Convert(color, out RawColor4 color4);
            SolidColorBrush brush = IsDisposing
                ? null
                : new SolidColorBrush(_d2d.Context, color4);
            return new DxSolidColorBrush(this, brush);
        }

        public void FillRectangle(IBrush brush, in RectangleF rectangle)
        {
            CheckThread();
            if (!IsDrawing) return;

            if (!(brush is IDxBrush dxBrush)) throw new ArgumentOutOfRangeException(nameof(brush), Error_FactoryObject);
            ConvertToPixels(rectangle, out RawRectangleF rawRectangle);
            _d2d.Context.FillRectangle(rawRectangle, dxBrush.Brush);
        }
    }
}

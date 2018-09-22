using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace TerminalVelocity.Direct2D
{
    public class ChromeRenderer
    {
        private struct NonClientMetrics
        {
            public RawRectangleF WindowFrame;
            public RawRectangleF SizeFrame;
            public RawRectangleF Caption;
            public RawRectangleF Tabs;
        }

        private readonly Direct2DRenderer _renderer;
        private NonClientMetrics _metrics;
        private NetCoreEx.Geometry.Size _previousSize;

        public ChromeRenderer(Direct2DRenderer renderer)
        {
            _renderer = renderer;
        }

        public void Render()
        {
            UpdateMetrics();

            var context = _renderer.Direct2DContext;
            using (var windowBrush = new SolidColorBrush(context, _renderer.Theme.Window))
            {
                // Caption + Tabs
                context.FillRectangle(new RawRectangleF(
                    _metrics.WindowFrame.Left, _metrics.WindowFrame.Top,
                    _metrics.WindowFrame.Right, _metrics.Caption.Bottom
                ), windowBrush);
                
                // Left Sizer
                context.FillRectangle(new RawRectangleF(
                    _metrics.WindowFrame.Left, _metrics.WindowFrame.Top,
                    _metrics.SizeFrame.Left, _metrics.WindowFrame.Bottom
                ), windowBrush);
                
                // Right Sizer
                context.FillRectangle(new RawRectangleF(
                    _metrics.SizeFrame.Right, _metrics.WindowFrame.Top,
                    _metrics.WindowFrame.Right, _metrics.WindowFrame.Bottom
                ), windowBrush);
                
                // Bottom Sizer
                context.FillRectangle(new RawRectangleF(
                    _metrics.WindowFrame.Left, _metrics.SizeFrame.Bottom,
                    _metrics.WindowFrame.Right, _metrics.WindowFrame.Bottom
                ), windowBrush);
            }
        }

        public WinApi.User32.HitTestResult HitTest(RawPoint point)
        {
            if  (point.X <= _metrics.SizeFrame.Left)
            {
                if (point.Y <= _metrics.SizeFrame.Top)
                    return WinApi.User32.HitTestResult.HTTOPLEFT;
                else if (point.Y >= _metrics.SizeFrame.Bottom)
                    return WinApi.User32.HitTestResult.HTBOTTOMLEFT;
                return WinApi.User32.HitTestResult.HTLEFT;
            }
            else if (point.X >= _metrics.SizeFrame.Right)
            {
                if (point.Y <= _metrics.SizeFrame.Top)
                    return WinApi.User32.HitTestResult.HTTOPRIGHT;
                else if (point.Y >= _metrics.SizeFrame.Bottom)
                    return WinApi.User32.HitTestResult.HTBOTTOMRIGHT;
                return WinApi.User32.HitTestResult.HTRIGHT;
            }
            else if (point.Y <= _metrics.SizeFrame.Top)
                return WinApi.User32.HitTestResult.HTTOP;
            else if (point.Y >= _metrics.SizeFrame.Bottom)
                return WinApi.User32.HitTestResult.HTBOTTOM;

            if (_metrics.Caption.Contains(point))
                return WinApi.User32.HitTestResult.HTCAPTION;

            return WinApi.User32.HitTestResult.HTCLIENT;
        }

        private void UpdateMetrics()
        {
            var size = _renderer.RenderWindow.GetClientSize();
            if (size.Width == _previousSize.Width && size.Height == _previousSize.Height)
                return;
            _previousSize = size;

            _metrics.WindowFrame = new RawRectangleF(
                0, 0,
                size.Width, size.Height
            );

            _metrics.SizeFrame = new RawRectangleF(
                SystemMetrics.SizeFrameWidth, SystemMetrics.SizeFrameHeight,
                size.Width - SystemMetrics.SizeFrameWidth, size.Height - SystemMetrics.SizeFrameHeight
            );

            _metrics.Caption = new RawRectangleF(
                _metrics.SizeFrame.Left, _metrics.SizeFrame.Top,
                _metrics.SizeFrame.Right, _metrics.SizeFrame.Top + SystemMetrics.CaptionButtonHeight * 2
            );

            _metrics.Tabs = new RawRectangleF(
                _metrics.SizeFrame.Left + SystemMetrics.CaptionButtonWidth * 2, _metrics.Caption.Bottom,
                _metrics.SizeFrame.Right - SystemMetrics.CaptionButtonWidth, _metrics.Caption.Bottom + SystemMetrics.CaptionButtonHeight
            );
        }
    }
}
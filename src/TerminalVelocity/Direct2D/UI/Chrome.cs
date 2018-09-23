using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace TerminalVelocity.Direct2D.UI
{
    internal struct Chrome : IDisposable
    {
        private readonly Direct2DRenderer _renderer;

        private RectangleF _frame;
        private RectangleF _sizeFrame;
        private RectangleF _title;
        private RectangleF _captionFrame;
        private RectangleF _client;
        
        private SysMenu _sysMenu;
        private NCButton _minButton;
        private MinMaxButton _maxButton;
        private NCButton _closeButton;
        private TextDisplay _text;
        
        private Stopwatch _frameTimer;

        public Chrome(Direct2DRenderer renderer)
        {
            _renderer = renderer;
            _frame = default;
            _sizeFrame = default;
            _captionFrame = default;
            _title = default;
            _client = default;

            _sysMenu = new SysMenu(renderer);
            _minButton = new NCButton(
                renderer, 2, IconFactory.MinButton, t => t.ChromeButtonHover, 
                WinApi.User32.HitTestResult.HTMINBUTTON, WinApi.User32.SysCommand.SC_MINIMIZE);
            _maxButton = new MinMaxButton(renderer, 1);
            _closeButton = new NCButton(
                renderer, 0, IconFactory.CloseButton, t => t.ChromeCloseButtonHover, 
                WinApi.User32.HitTestResult.HTCLOSE, WinApi.User32.SysCommand.SC_CLOSE);
            _text = new TextDisplay(renderer);

            _frameTimer = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            using (_minButton)
            using (_maxButton)
            using (_closeButton)
            using (_text)
            { }
        }

        public void Layout(RectangleF container)
        {
            _frame = container;

            _sizeFrame = RectangleFUtils.Rect(
                _frame.Left + WindowsLayout.SizeFrameWidth, _frame.Top + WindowsLayout.SizeFrameHeight,
                _frame.Right - WindowsLayout.SizeFrameWidth, _frame.Bottom - WindowsLayout.SizeFrameHeight
            );

            _captionFrame = RectangleFUtils.Rect(
                _frame.Left, _frame.Top,
                _frame.Right, _frame.Top + WindowsLayout.CaptionBarHeight + WindowsLayout.SizeFrameHeight
            );

            _title = RectangleFUtils.Rect(
                _frame.Left, _frame.Top,
                _frame.Right, _captionFrame.Bottom + WindowsLayout.CaptionBarHeight
            );

            _client = RectangleFUtils.Rect(
                _frame.Left, _title.Bottom,
                _frame.Right, _frame.Bottom
            );

            _sysMenu.Layout(_title);
            _minButton.Layout(_captionFrame);
            _maxButton.Layout(_captionFrame);
            _closeButton.Layout(_captionFrame);
            _text.Layout(_client);
        }
        
        public void HitTest(ref HitTestResult result, Point point)
        {
            // Window padding.
            if (!_frame.Contains(point))
            {
                result.Region = WinApi.User32.HitTestResult.HTNOWHERE;
                return;
            }

            if (_captionFrame.Contains(point) && !result.IsInBounds)
                result.Region = WinApi.User32.HitTestResult.HTCAPTION;

            _text.HitTest(ref result, point);

            if (point.X <= _sizeFrame.Left)
            {
                if (point.Y <= _sizeFrame.Top)
                    result.Region = WinApi.User32.HitTestResult.HTTOPLEFT;
                else if (point.Y >= _sizeFrame.Bottom)
                    result.Region = WinApi.User32.HitTestResult.HTBOTTOMLEFT;
                else
                    result.Region = WinApi.User32.HitTestResult.HTLEFT;
            }
            else if (point.X >= _sizeFrame.Right)
            {
                if (point.Y <= _sizeFrame.Top)
                    result.Region = WinApi.User32.HitTestResult.HTTOPRIGHT;
                else if (point.Y >= _sizeFrame.Bottom)
                    result.Region = WinApi.User32.HitTestResult.HTBOTTOMRIGHT;
                else
                    result.Region = WinApi.User32.HitTestResult.HTRIGHT;
            }
            else if (point.Y <= _sizeFrame.Top)
                result.Region = WinApi.User32.HitTestResult.HTTOP;
            else if (point.Y >= _sizeFrame.Bottom)
                result.Region = WinApi.User32.HitTestResult.HTBOTTOM;
                
            _sysMenu.HitTest(ref result, point);
            _minButton.HitTest(ref result, point);
            _maxButton.HitTest(ref result, point);
            _closeButton.HitTest(ref result, point);
        }

        internal bool Event<T>(T evt)
            where T : struct
        {
            return _text.Event(ref evt)
                || _sysMenu.Event(ref evt)
                || _minButton.Event(ref evt)
                || _maxButton.Event(ref evt)
                || _closeButton.Event(ref evt);
        }

        public void Render()
        {
            var context = _renderer.Direct2DContext;
            context.Transform = Matrix3x2.Identity;
            using (var windowBrush = new SolidColorBrush(context, _renderer.Theme.ChromeBackground))
            using (var windowText = new SolidColorBrush(context, _renderer.Theme.ChromeText))
            {
                var dpiHeight = _renderer.Direct2DFactory.DesktopDpi.Height / 48;

                // Caption
                context.FillRectangle(RectangleFUtils.Rect(
                    _frame.Left, _frame.Top,
                    _frame.Right, _title.Bottom
                ), windowBrush);

                // Titlebar Text
                using (var tf = new TextFormat(_renderer.DirectWriteFactory, WindowsLayout.CaptionFont, WindowsLayout.CaptionTextHeight / dpiHeight)
                {
                    ParagraphAlignment = ParagraphAlignment.Center,
                    TextAlignment = TextAlignment.Center
                })
                {
                    var text = "Terminal Velocity";
                    if (_renderer.Preferences.DisplayFps)
                    {
                        var fps = _frameTimer.Elapsed.TotalMilliseconds;
                        if (fps <= 0)
                            text += " - ∞fps";
                        else
                            text += $" - {1000F / fps:0.00}";
                        _frameTimer.Restart();
                    }

                    context.DrawText(text, tf, _captionFrame, windowText);
                }

                _sysMenu.Render();
                _minButton.Render();
                _maxButton.Render();
                _closeButton.Render();
                _text.Render();
            }
        }
    }
}
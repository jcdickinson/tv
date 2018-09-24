using System;
using System.Composition;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D.UI
{
    [Shared, Export]
    public sealed class Chrome
    {
        private readonly DeviceContext _context;

        private readonly Configurable<Brush> _chromeBackground;
        private readonly Configurable<Brush> _chromeText;
        private readonly Configurable<TextFormat> _captionTextTextFormat;
        private readonly Configurable<bool> _displayFps;
        private readonly Configurable<System.Drawing.Size> _sizeFrameSize;
        private readonly Configurable<System.Drawing.Size> _captionBarSize;

        private readonly NCButton _minButton;
        private readonly NCButton _maxButton;
        private readonly NCButton _closeButton;
        private readonly SysMenu _sysMenu;

        private RectangleF _frame;
        private RectangleF _sizeFrame;
        private RectangleF _title;
        private RectangleF _captionFrame;
        private RectangleF _client;
        
        private Stopwatch _frameTimer;

        [ImportingConstructor]
        public Chrome(
            [Import(BrushProvider.ChromeBackgroundContract)] Configurable<Brush> chromeBackground,
            [Import(BrushProvider.ChromeTextContract)] Configurable<Brush> chromeText,
            [Import(FontProvider.CaptionTextContract)] Configurable<TextFormat> captionTextTextFormat,
            [Import(Behavior.DisplayFpsContract)] Configurable<bool> displayFps,
            [Import(WindowsMetricsProvider.SizeFrameContract)] Configurable<System.Drawing.Size> sizeFrameSize,
            [Import(WindowsMetricsProvider.CaptionBarContract)] Configurable<System.Drawing.Size> captionBarSize,
            [Import] DeviceContext context,
            [Import] MinButton minButton,
            [Import] MinMaxButton maxButton,
            [Import] CloseButton closeButton,
            [Import] SysMenu sysMenu)
        {
            _chromeBackground = chromeBackground;
            _chromeText = chromeText;
            _captionTextTextFormat = captionTextTextFormat;
            _displayFps = displayFps;
            _sizeFrameSize = sizeFrameSize;
            _captionBarSize = captionBarSize;

            _sysMenu = sysMenu;
            _minButton = minButton;
            _maxButton = maxButton;
            _closeButton = closeButton;
            
            _context = context;
            _frameTimer = Stopwatch.StartNew();
        }

        public void Layout(in RectangleF container)
        {
            _frame = container;

            _sizeFrame = RectangleFUtils.Rect(
                _frame.Left + _sizeFrameSize.Value.Width, _frame.Top + _sizeFrameSize.Value.Height,
                _frame.Right - _sizeFrameSize.Value.Width, _frame.Bottom - _sizeFrameSize.Value.Height
            );

            _captionFrame = RectangleFUtils.Rect(
                _frame.Left, _frame.Top,
                _frame.Right, _frame.Top + _captionBarSize.Value.Height + _sizeFrameSize.Value.Height
            );

            _title = RectangleFUtils.Rect(
                _frame.Left, _frame.Top,
                _frame.Right, _captionFrame.Bottom + _captionBarSize.Value.Height
            );

            _client = RectangleFUtils.Rect(
                _frame.Left, _title.Bottom,
                _frame.Right, _frame.Bottom
            );

            _sysMenu.Layout(_title);
            _minButton.Layout(_captionFrame);
            _maxButton.Layout(_captionFrame);
            _closeButton.Layout(_captionFrame);
        }

        internal void HitTest(ref HitTestEvent evt)
        {
            if (_captionFrame.Contains(evt.Point) && !evt.IsInBounds)
                evt.Region = WinApi.User32.HitTestResult.HTCAPTION;
                
            _sysMenu.HitTest(ref evt);

            if (evt.Point.X <= _sizeFrame.Left)
            {
                if (evt.Point.Y <= _sizeFrame.Top)
                    evt.Region = WinApi.User32.HitTestResult.HTTOPLEFT;
                else if (evt.Point.Y >= _sizeFrame.Bottom)
                    evt.Region = WinApi.User32.HitTestResult.HTBOTTOMLEFT;
                else
                    evt.Region = WinApi.User32.HitTestResult.HTLEFT;
            }
            else if (evt.Point.X >= _sizeFrame.Right)
            {
                if (evt.Point.Y <= _sizeFrame.Top)
                    evt.Region = WinApi.User32.HitTestResult.HTTOPRIGHT;
                else if (evt.Point.Y >= _sizeFrame.Bottom)
                    evt.Region = WinApi.User32.HitTestResult.HTBOTTOMRIGHT;
                else
                    evt.Region = WinApi.User32.HitTestResult.HTRIGHT;
            }
            else if (evt.Point.Y <= _sizeFrame.Top)
                evt.Region = WinApi.User32.HitTestResult.HTTOP;
            else if (evt.Point.Y >= _sizeFrame.Bottom)
                evt.Region = WinApi.User32.HitTestResult.HTBOTTOM;
                
            _minButton.HitTest(ref evt);
            _maxButton.HitTest(ref evt);
            _closeButton.HitTest(ref evt);
            
            // Window padding.
            if (!_frame.Contains(evt.Point))
                evt.Region = WinApi.User32.HitTestResult.HTNOWHERE;
        }

        public void Render()
        {
            _context.Transform = Matrix3x2.Identity;

            _context.FillRectangle(RectangleFUtils.Rect(
                _frame.Left, _frame.Top,
                _frame.Right, _title.Bottom
            ), _chromeBackground);

            _captionTextTextFormat.Value.ParagraphAlignment = ParagraphAlignment.Center;
            _captionTextTextFormat.Value.TextAlignment = TextAlignment.Center;

            var text = "Terminal Velocity";
            if (_displayFps)
            {
                var fps = _frameTimer.Elapsed.TotalMilliseconds;
                if (fps <= 0)
                    text += " - ∞fps";
                else
                    text += $" - {1000F / fps:0.00}fps";
                _frameTimer.Restart();
            }

            _context.DrawText(text, _captionTextTextFormat, _captionFrame, _chromeText);

            _sysMenu.Render();
            _minButton.Render();
            _maxButton.Render();
            _closeButton.Render();
        }
    }
}
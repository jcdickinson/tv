using System;
using System.Composition;
using System.Composition.Hosting;
using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    [Export]
    public sealed class MinMaxButton : NCButton
    {
        [Import(EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> OnEmulateMessage { private get; set; }

        [Import(SizeEvent.ContractName)]
        public Event<SizeEvent> OnSize
        {
            set => value.Subscribe((ref SizeEvent evt) =>
            {
                if (evt.Flag == WindowSizeFlag.SIZE_MAXIMIZED)
                    _isMaximized = true;
                else if (evt.Flag == WindowSizeFlag.SIZE_RESTORED)
                    _isMaximized = false;
            });
        }

        protected override HitTestResult Region => _isMaximized
            ? HitTestResult.HTMINBUTTON
            : HitTestResult.HTMAXBUTTON;

        protected override Geometry Geometry => _isMaximized
            ? _minGeometry
            : _maxGeometry;

        protected override Brush HoverBrush => _isMaximized
            ? _minBrush
            : _maxBrush;

        private readonly Configurable<Brush> _maxBrush;
        private readonly Configurable<Brush> _minBrush;
        private readonly Geometry _maxGeometry;
        private readonly Geometry _minGeometry;

        private bool _isMaximized;

        [ImportingConstructor]
        public MinMaxButton(
            [Import(WindowsMetricsProvider.CaptionBarContract)] Configurable<Size> captionBarSize, 
            [Import(WindowsMetricsProvider.CaptionButtonContract)] Configurable<Size> captionButtonSize, 
            [Import(BrushProvider.ChromeTextContract)] Configurable<Brush> foregroundBrush,
            [Import(BrushProvider.ChromeMaxButtonContract)] Configurable<Brush> maxBrush,
            [Import(BrushProvider.ChromeRestoreButtonContract)] Configurable<Brush> minBrush,
            [Import(IconProvider.MaxButtonContract)] Geometry maxGeometry,
            [Import(IconProvider.RestoreButtonContract)] Geometry minGeometry,
            [Import] DeviceContext context) 
            : base(captionBarSize, captionButtonSize, foregroundBrush, context, 1)
        {
            _maxBrush = maxBrush;
            _minBrush = minBrush;
            _maxGeometry = maxGeometry;
            _minGeometry = minGeometry;
        }

        protected override void OnClick()
        {
            if (_isMaximized)
                OnEmulateMessage.Publish(EmulateMessageEvent.CreateSysCommand(SysCommand.SC_RESTORE));
            else
                OnEmulateMessage.Publish(EmulateMessageEvent.CreateSysCommand(SysCommand.SC_MAXIMIZE));
        }
    }
}
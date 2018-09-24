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
    public sealed class MinButton : NCButton
    {
        [Import(EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> OnEmulateMessage { private get; set; }

        protected override HitTestResult Region => HitTestResult.HTCLOSE;

        protected override Geometry Geometry => _geometry;

        protected override Brush HoverBrush => _hoverBrush;

        private readonly Configurable<Brush> _hoverBrush;
        private readonly Geometry _geometry;
        

        [ImportingConstructor]
        public MinButton(
            [Import(WindowsMetricsProvider.CaptionBarContract)] Configurable<Size> captionBarSize, 
            [Import(WindowsMetricsProvider.CaptionButtonContract)] Configurable<Size> captionButtonSize, 
            [Import(BrushProvider.ChromeTextContract)] Configurable<Brush> foregroundBrush,
            [Import(BrushProvider.ChromeMinButtonContract)] Configurable<Brush> hoverBrush,
            [Import(IconProvider.MinButtonContract)] Geometry geometry,
            [Import] DeviceContext context) 
            : base(captionBarSize, captionButtonSize, foregroundBrush, context, 2)
        {
            _hoverBrush = hoverBrush;
            _geometry = geometry;
        }

        protected override void OnClick()
        {
            OnEmulateMessage.Publish(EmulateMessageEvent.CreateSysCommand(SysCommand.SC_MINIMIZE));
        }
    }
}
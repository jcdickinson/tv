using System;
using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    [Export]
    public sealed class SysMenu
    {
        private readonly DeviceContext _context;
        private readonly Geometry _icon;
        private readonly Configurable<Brush> _iconBrush;
        private readonly Configurable<System.Drawing.Size> _captionBarSize;
        
        private RectangleF _frame;
        private RectangleF _icoFrame;
        private bool _clicking;
        private bool _hover;

        [ImportingConstructor]
        public SysMenu(
            [Import] DeviceContext context,
            [Import(IconProvider.LogoContract)] Geometry icon,
            [Import(BrushProvider.LogoContract)] Configurable<Brush> iconBrush,
            [Import(WindowsMetricsProvider.CaptionBarContract)] Configurable<System.Drawing.Size> captionBarSize
        )
        {
            _context = context;
            _icon = icon;
            _iconBrush = iconBrush;
            _captionBarSize = captionBarSize;
        }

        public void Layout(in RectangleF container)
        {
            const float padding = 0.05f;

            _frame = RectangleFUtils.Rect(
                container.Left, container.Top,
                container.Left + _captionBarSize.Value.Height * 2, container.Top + _captionBarSize.Value.Height * 2
            );

            _icoFrame = RectangleFUtils.Rect(
                _frame.Left + _frame.Width * padding, _frame.Top + _frame.Height * padding,
                _frame.Right - _frame.Width * padding, _frame.Bottom - _frame.Height * padding
            );
        }

        public void HitTest(ref HitTestEvent evt)
        {
            if (_frame.Contains(evt.Point))
            {
                _hover = true;
                evt.Region = WinApi.User32.HitTestResult.HTSYSMENU;
            }
            else
            {
                _hover = false;
                _clicking = false;
            }
        }

        public void Render()
        {
            _context.Transform = Matrix3x2.Scaling(_icoFrame.Width / 10) * Matrix3x2.Translation(_icoFrame.Location);
            _context.FillGeometry(_icon, _iconBrush);
        }
    }
}
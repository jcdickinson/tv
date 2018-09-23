using System;
using SharpDX;
using SharpDX.Direct2D1;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    internal struct SysMenu : IDisposable
    {
        private readonly Direct2DRenderer _renderer;
        private readonly Geometry _geometry;
        
        private RectangleF _frame;
        private RectangleF _icon;
        private bool _clicking;
        private bool _hover;

        public SysMenu(Direct2DRenderer renderer)
        {
            _renderer = renderer;
            _geometry = IconFactory.AppIcon(renderer.Direct2DFactory);
            _frame = default;
            _icon = default;
            _clicking = false;
            _hover = true;
        }

        public void Dispose()
        {
            _geometry.Dispose();
        }
        
        public void Layout(RectangleF container)
        {
            const float padding = 0.05f;

            _frame = RectangleFUtils.Rect(
                container.Left, container.Top,
                container.Left + WindowsLayout.CaptionBarHeight * 2, container.Top + WindowsLayout.CaptionBarHeight * 2
            );

            _icon = RectangleFUtils.Rect(
                _frame.Left + _frame.Width * padding, _frame.Top + _frame.Height * padding,
                _frame.Right - _frame.Width * padding, _frame.Bottom - _frame.Height * padding
            );
        }

        public void HitTest(ref HitTestResult result, Point point)
        {
            if (_frame.Contains(point))
            {
                _hover = true;
                result.Region = WinApi.User32.HitTestResult.HTSYSMENU;
            }
            else
            {
                _hover = false;
                _clicking = false;
            }
        }

        internal bool Event<T>(ref T evt) => false;

        public void Render()
        {
            var context = _renderer.Direct2DContext;

            context.Transform = Matrix3x2.Scaling(_icon.Width / 10) * Matrix3x2.Translation(_icon.Location);

            using (var brush = new SolidColorBrush(context, _renderer.Theme.Logo))
            {
                context.FillGeometry(_geometry, brush);
            }
        }
    }
}
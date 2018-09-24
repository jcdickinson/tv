using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    [Export]
    public abstract class NCButton
    {
        [Import(RenderEvent.ContractName)]
        public Event<RenderEvent> OnRender { private get; set; }

        private readonly DeviceContext _context;
        private readonly Configurable<System.Drawing.Size> _captionButtonSize;
        private readonly Configurable<System.Drawing.Size> _captionBarSize;
        private readonly Configurable<Brush> _foregroundBrush;
        private readonly int _index;

        protected abstract WinApi.User32.HitTestResult Region { get; }
        protected abstract Geometry Geometry { get; }
        protected abstract Brush HoverBrush { get; }

        private RectangleF _frame;
        private bool _hover;
        private bool _clicking;

        public NCButton(
            [Import(WindowsMetricsProvider.CaptionBarContract)] Configurable<System.Drawing.Size> captionBarSize,
            [Import(WindowsMetricsProvider.CaptionButtonContract)] Configurable<System.Drawing.Size> captionButtonSize,
            [Import(BrushProvider.ChromeTextContract)] Configurable<Brush> foregroundBrush,
            [Import] DeviceContext context,
            int index
        )
        {
            _context = context;
            _captionButtonSize = captionButtonSize;
            _captionBarSize = captionBarSize;
            _foregroundBrush = foregroundBrush;
            _index = index;
        }

        public void Layout(in RectangleF container)
        {
            _frame = RectangleFUtils.Rect(
                container.Right - _captionButtonSize.Value.Width * (_index + 1), container.Top,
                container.Right - _captionButtonSize.Value.Width * (_index + 0), container.Top + _captionBarSize.Value.Height
            );
        }

        public void HitTest(ref HitTestEvent evt)
        {
            var render = false;

            if (_frame.Contains(evt.Point))
            {
                render = !_hover;
                _hover = true;
                evt.Region = Region;
            }
            else
            {
                render = _hover;
                _hover = false;
                _clicking = false;
            }

            if (render) OnRender.Publish(new RenderEvent());
        }
        

        [Import(MouseButtonEvent.ContractName)]
        public Event<MouseButtonEvent> OnMouseButton
        {
            set => value.Subscribe((ref MouseButtonEvent evt) =>
            {
                if (evt.Button == WinApi.Windows.MouseButton.Left)
                {
                    if (_hover && evt.IsButtonDown)
                        _clicking = true;
                    else if (_hover && _clicking && !evt.IsButtonDown)
                    {
                        _clicking = false;
                        OnClick();
                    }
                }
            });
        }

        protected abstract void OnClick();

        public void Render()
        {
            if (_hover)
            {
                _context.Transform = Matrix3x2.Identity;
                _context.FillRectangle(_frame, HoverBrush);
            }

            var offset = new Vector2(
                _frame.Left + (float)Math.Floor((_frame.Width - 10) / 2) + 0.5f,
                _frame.Top + (float)Math.Floor((_frame.Height - 10) / 2) + 0.5f
            );

            _context.Transform = Matrix3x2.Scaling(10) * Matrix3x2.Translation(offset);
            _context.DrawGeometry(Geometry, _foregroundBrush, 0.1f);
        }
    }
}
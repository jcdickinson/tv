using System;
using SharpDX;
using SharpDX.Direct2D1;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    internal struct NCButton : IDisposable
    {
        private readonly Direct2DRenderer _renderer;
        private readonly Geometry _geometry;
        private readonly WinApi.User32.HitTestResult _region;
        private readonly int _index;
        private readonly Func<Theme, Color4> _hoverColor;
        private readonly SysCommand _sysCommand;
        
        private RectangleF _frame;
        private bool _hover;
        private bool _clicking;

        public NCButton(
            Direct2DRenderer renderer, 
            int index, 
            Func<Factory, Geometry> icon, 
            Func<Theme, Color4> hoverColor, 
            WinApi.User32.HitTestResult region,
            SysCommand sysCommand)
        {
            _index = index;
            _renderer = renderer;
            _region = region;
            _geometry = icon(renderer.Direct2DFactory);
            _frame = default;
            _hover = false;
            _hoverColor = hoverColor;
            _clicking = false;
            _sysCommand = sysCommand;
        }

        public void Dispose()
        {
            _geometry.Dispose();
        }
        
        public void Layout(RectangleF container)
        {
            _frame = RectangleFUtils.Rect(
                container.Right - WindowsLayout.CaptionButtonWidth * (_index + 1), container.Top,
                container.Right - WindowsLayout.CaptionButtonWidth * (_index + 0), container.Top + WindowsLayout.CaptionBarHeight
            );
        }

        public void HitTest(ref HitTestResult result, Point point)
        {
            if (_frame.Contains(point))
            {
                if (!_hover) result.Flags |= HitTestFlags.Repaint;
                _hover = true;
                result.Region = _region;
            }
            else
            {
                if (_hover) result.Flags |= HitTestFlags.Repaint;
                _hover = false;
                _clicking = false;
            }
        }

        internal bool Event<T>(ref T evt) 
            where T : struct
        {
            if (evt is WinApi.Windows.MouseButtonPacket mouseButton)
                return MouseButtonEvent(ref mouseButton);
            return false;
        }

        private bool MouseButtonEvent(ref WinApi.Windows.MouseButtonPacket evt)
        {
            if (evt.Button == WinApi.Windows.MouseButton.Left)
            {
                if (_hover && evt.IsButtonDown)
                    _clicking = true;
                else if (_hover && _clicking && !evt.IsButtonDown)
                {
                    _renderer.RenderWindow.SendSysCommand(_sysCommand);
                    _clicking = false;
                }
            }
            return false;
        }

        public void Render()
        {
            var context = _renderer.Direct2DContext;

            if (_hover)
            {
                context.Transform = Matrix3x2.Identity;
                using (var brush = new SolidColorBrush(context, _hoverColor(_renderer.Theme)))
                {
                    context.FillRectangle(_frame, brush);
                }
            }

            var offset = new Vector2(
                _frame.Left + (float)Math.Floor((_frame.Width - 10) / 2) + 0.5f,
                _frame.Top +(float)Math.Floor((_frame.Height - 10) / 2) + 0.5f
            );

            context.Transform = Matrix3x2.Scaling(10) * Matrix3x2.Translation(offset);

            using (var brush = new SolidColorBrush(context, _renderer.Theme.ChromeText))
            {
                context.DrawGeometry(_geometry, brush, 0.1f);
            }
        }
    }
}
using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Direct2D.UI;

namespace TerminalVelocity.Direct2D
{
    public class ChromeRenderer : IDisposable
    {
        private readonly Direct2DRenderer _renderer;
        private NetCoreEx.Geometry.Size _previousSize;

        private Chrome _chrome;

        public ChromeRenderer(Direct2DRenderer renderer)
        {
            _renderer = renderer;
            _chrome = new Chrome(renderer);
        }

        public void Dispose()
        {
            _chrome.Dispose();
        }

        public void Render()
        {
            Layout();
            _chrome.Render();
        }

        public HitTestResult HitTest(Point point)
        {
            Layout();
            var result = new HitTestResult();
            _chrome.HitTest(ref result, point);
            return result;
        }

        public bool Event<T>(ref T evt)
            where T : struct
            => _chrome.Event(evt);

        private void Layout()
        {
            var size = _renderer.RenderWindow.GetClientSize();
            if (size.Width == _previousSize.Width && size.Height == _previousSize.Height)
                return;
            _previousSize = size;

            var rectangle = new RectangleF(0, 0, size.Width, size.Height);

            if (_renderer.RenderWindow.Placement.ShowCmd == WinApi.User32.ShowWindowCommands.SW_MAXIMIZE)
            {
                rectangle.Left += WindowsLayout.WindowPaddingWidth * 2;
                rectangle.Top += WindowsLayout.WindowPaddingHeight * 2;
                rectangle.Width -= WindowsLayout.WindowPaddingWidth * 2;
                rectangle.Height -= WindowsLayout.WindowPaddingHeight * 2;
            }

            _chrome.Layout(rectangle);
        }

    }
}
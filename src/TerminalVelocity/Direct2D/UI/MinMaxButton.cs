using System;
using SharpDX;
using SharpDX.Direct2D1;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    internal struct MinMaxButton : IDisposable
    {
        private readonly Direct2DRenderer _renderer;
        private NCButton _minButton;
        private NCButton _maxButton;

        private bool IsMaximized => _renderer.RenderWindow.Placement.ShowCmd == ShowWindowCommands.SW_MAXIMIZE;

        public MinMaxButton(
            Direct2DRenderer renderer, 
            int index)
        {
            _renderer = renderer;

            _minButton = new NCButton(
                renderer, index, IconFactory.RestoreButton, t => t.ChromeButtonHover, 
                WinApi.User32.HitTestResult.HTMAXBUTTON, WinApi.User32.SysCommand.SC_RESTORE);
            _maxButton = new NCButton(
                renderer, index, IconFactory.MaxButton, t => t.ChromeButtonHover,
                WinApi.User32.HitTestResult.HTMAXBUTTON, WinApi.User32.SysCommand.SC_MAXIMIZE);
        }

        public void Dispose()
        {
            using (_minButton)
            using (_maxButton)
            {}
        }
        
        public void Layout(RectangleF container)
        {
            if (IsMaximized) _minButton.Layout(container);
            else _maxButton.Layout(container);
        }

        public void HitTest(ref HitTestResult result, Point point) 
        {
            if (IsMaximized) _minButton.HitTest(ref result, point);
            else _maxButton.HitTest(ref result, point);
        }

        internal bool Event<T>(ref T evt) 
            where T : struct
        {
            if (IsMaximized) return _minButton.Event(ref evt);
            else return _maxButton.Event(ref evt);
        }

        public void Render()
        {
            if (IsMaximized) _minButton.Render();
            else _maxButton.Render();
        }
    }
}
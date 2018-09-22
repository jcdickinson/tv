using System;
using SharpDX.Mathematics.Interop;
using WinApi.DxUtils.Component;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public sealed class RenderWindow : EventedWindowCore
    {
        private static readonly WindowFactory _factory;

        static RenderWindow()
        {
            _factory = WindowFactory.Create(
                className: "TerminalVelocity",
                hBgBrush: IntPtr.Zero);
        }

        public static RenderWindow Create(Direct2DRenderer renderer)
        {
            return _factory.CreateWindow(() => new RenderWindow(renderer), "Terminal Velocity",
                constructionParams: new ConstructionParams(),
                width: 500,
                height: 500,
                styles: WindowStyles.WS_POPUP | WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_MINIMIZEBOX,
                exStyles: WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_NOREDIRECTIONBITMAP);
        }

        public Dx11Component DirectX { get; }
        private readonly Direct2DRenderer _renderer;

        private RenderWindow(Direct2DRenderer renderer)
        {
            DirectX = new Dx11Component();
            _renderer = renderer;
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            DirectX.Initialize(Handle, GetClientSize());
            base.OnCreate(ref packet);
        }

        protected override void OnSize(ref SizePacket packet)
        {
            DirectX.Resize(packet.Size);
            base.OnSize(ref packet);
        }

        protected override void OnPaint(ref PaintPacket packet)
        {
            _renderer.Render();
            Validate();
        }

        protected override void OnNcHitTest(ref NcHitTestPacket packet)
        {
            var position = this.GetWindowRect();
            var point = new RawPoint(
                packet.Point.X - position.Left,
                packet.Point.Y - position.Top);
            packet.Result = _renderer.Chrome.HitTest(point);
        }

        private NetCoreEx.Geometry.Rectangle _old;
        protected override void OnSysCommand(ref SysCommandPacket packet)
        {
            if (packet.Command == SysCommand.SC_MAXIMIZE ||
                packet.Command == (SysCommand)0xF032)
            {
                var monitor = SystemMetrics.GetMonitorWorkingArea(Handle);
                _old = GetWindowRect();
                SetPosition(monitor.Left, monitor.Top);
                SetSize(monitor.Width, monitor.Height);
            }
            else if (packet.Command == SysCommand.SC_RESTORE ||
                packet.Command == (SysCommand)61730)
            {
                SetPosition(_old.Left, _old.Top);
                SetSize(_old.Width, _old.Height);
            }
            else
            {
                Console.WriteLine(packet.Command);
                base.OnSysCommand(ref packet);
            }
        }

        // protected override void OnGetMinMaxInfo(ref MinMaxInfoPacket packet)
        // {}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (DirectX)
                { }
            }
            base.Dispose(disposing);
        }
    }
}
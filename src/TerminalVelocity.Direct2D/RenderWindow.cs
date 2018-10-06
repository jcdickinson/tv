using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using NetCoreEx.BinaryExtensions;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Eventing;
using TerminalVelocity.Renderer.Events;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public sealed class RenderWindow : EventedWindowCore, IConstructionParamsProvider
    {
        private sealed class RenderWindowConstructionParams : IConstructionParams
        {
            public WindowStyles Styles => WindowStyles.WS_OVERLAPPEDWINDOW;

            public WindowExStyles ExStyles => WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_NOREDIRECTIONBITMAP;

            public uint ControlStyles => 0;

            public int Width => 500;

            public int Height => 500;

            public int X => 10;

            public int Y => 10;

            public IntPtr ParentHandle => IntPtr.Zero;

            public IntPtr MenuHandle => IntPtr.Zero;
        }

        private readonly MouseButtonEvent _mouseButtonEvent;
        private readonly RenderEvent _renderEvent;
        private readonly SysCommandEvent _sysCommandEvent;
        private readonly ResizeEvent _resizeEvent;
        private readonly CloseEvent _closeEvent;
        private readonly InitializeEvent _initializeEvent;
        private readonly CreateEvent _createEvent;

        private SizeF _dpiScale = new SizeF(1, 1);

        public RenderWindow(
            MouseButtonEvent mouseButtonEvent = null,
            RenderEvent renderEvent = null,
            SysCommandEvent sysCommandEvent = null,
            ResizeEvent resizeEvent = null,
            CloseEvent closeEvent = null,
            InitializeEvent initializeEvent = null,
            CreateEvent createEvent = null)
        {
            _mouseButtonEvent = mouseButtonEvent;
            _renderEvent = renderEvent;
            _sysCommandEvent = sysCommandEvent;
            _resizeEvent = resizeEvent;
            _closeEvent = closeEvent;
            _initializeEvent = initializeEvent;
            _createEvent = createEvent;
        }

        IConstructionParams IConstructionParamsProvider.GetConstructionParams()
            => new RenderWindowConstructionParams();

        private ValueTask<EventStatus> OnEmulateMessage(EmulateMessageEventData e, CancellationToken cancellationToken)
        {
            User32Methods.SendMessage(
                Handle,
                (uint)e.Message.Id,
                e.Message.WParam,
                e.Message.WParam);
            return new ValueTask<EventStatus>(EventStatus.Continue);
        }

        protected override void OnMessage(ref WindowMessage msg)
        {
            switch (msg.Id)
            {
                case WM.DPICHANGED:
                    OnDpiChanged(ref msg);
                    break;
                default:
                    base.OnMessage(ref msg);
                    break;
            }
        }

        private void OnDpiChanged(ref WindowMessage msg)
        {
            msg.WParam.BreakSafeInt32To16Signed(out var yAxis, out var xAxis);
            _dpiScale = new SizeF(
                xAxis / 96.0f,
                yAxis / 96.0f
            );

            base.OnMessage(ref msg);
        }

        protected override void OnSysCommand(ref SysCommandPacket packet)
        {
            _sysCommandEvent?.Publish(new SysCommandEventData(packet));
            base.OnSysCommand(ref packet);
        }

        protected override void OnSize(ref SizePacket packet)
        {
            Convert(packet.Size, out SizeF size);
            _resizeEvent?.Publish(new ResizeEventData(size));
            base.OnSize(ref packet);
        }

        protected override void OnClose(ref Packet packet)
        {
            _closeEvent?.Publish(new CloseEventData());
            base.OnClose(ref packet);
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            SetText("Terminal Velocity");

            const int LOGPIXELSX = 88;
            const int LOGPIXELSY = 90;

            IntPtr hdc = User32Methods.GetDC(Handle);
            _dpiScale = new SizeF(
                WinApi.Gdi32.Gdi32Methods.GetDeviceCaps(hdc, LOGPIXELSX) / 96.0f,
                WinApi.Gdi32.Gdi32Methods.GetDeviceCaps(hdc, LOGPIXELSY) / 96.0f
            );
            User32Methods.ReleaseDC(Handle, hdc);

            Convert(GetClientSize(), out SizeF size);
            _initializeEvent?.Publish(new InitializeEventData(Handle, size));
            _createEvent?.Publish(new CreateEventData());

            base.OnCreate(ref packet);
        }

        protected override void OnPaint(ref PaintPacket packet)
        {
            _renderEvent?.Publish(new RenderEventData());
            Validate();
        }

        private void Convert(in NetCoreEx.Geometry.Size sizeEx, out SizeF size)
        {
            SizeF dpi = _dpiScale;
            size = new SizeF(
                sizeEx.Width / _dpiScale.Width,
                sizeEx.Height / _dpiScale.Height);
        }
    }
}

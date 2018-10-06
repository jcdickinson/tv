using System;
using System.Threading;
using System.Threading.Tasks;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Eventing;
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
        private readonly CreatedEvent _createdEvent;

        public RenderWindow(
            MouseButtonEvent mouseButtonEvent,
            RenderEvent renderEvent,
            SysCommandEvent sysCommandEvent,
            ResizeEvent resizeEvent,
            CloseEvent closeEvent,
            CreatedEvent createdEvent)
        {
            _mouseButtonEvent = mouseButtonEvent ?? throw new ArgumentNullException(nameof(mouseButtonEvent));
            _renderEvent = renderEvent ?? throw new ArgumentNullException(nameof(renderEvent));
            _sysCommandEvent = sysCommandEvent ?? throw new ArgumentNullException(nameof(sysCommandEvent));
            _resizeEvent = resizeEvent ?? throw new ArgumentNullException(nameof(resizeEvent));
            _closeEvent = closeEvent ?? throw new ArgumentNullException(nameof(closeEvent));
            _createdEvent = createdEvent ?? throw new ArgumentNullException(nameof(createdEvent));

            // if (emulateMessageEvent == null) throw new ArgumentNullException(nameof(emulateMessageEvent));

            // emulateMessageEvent.Raised += OnEmulateMessage;
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

        protected override void OnSysCommand(ref SysCommandPacket packet)
        {
            _sysCommandEvent.Publish(new SysCommandEventData(packet));
            base.OnSysCommand(ref packet);
        }

        protected override void OnSize(ref SizePacket packet)
        {
            _resizeEvent.Publish(new ResizeEventData(packet));
            base.OnSize(ref packet);
        }

        protected override void OnClose(ref Packet packet)
        {
            _closeEvent.Publish(new CloseEventData());
            base.OnClose(ref packet);
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            _createdEvent?.Publish(new CreatedEventData(Handle, GetClientSize()));
            RedrawFrame();
            SetText("Terminal Velocity");

            base.OnCreate(ref packet);
        }

        protected override void OnPaint(ref PaintPacket packet)
        {
            _renderEvent.Publish(new RenderEventData());
            Validate();
        }
    }
}

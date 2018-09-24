using System;
using System.Collections.Generic;
using System.Composition;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.DxUtils.Component;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public sealed class RenderWindow : EventedWindowCore
    {
        [Import(HitTestEvent.ContractName)]
        public Event<HitTestEvent> HitTest { private get; set; }

        [Import(MouseButtonEvent.ContractName)]
        public Event<MouseButtonEvent> MouseButton { private get; set; }

        [Import(LayoutEvent.ContractName)]
        public Event<LayoutEvent> Layout { private get; set; }

        [Import(RenderEvent.ContractName)]
        public Event<RenderEvent> Render { private get; set; }

        [Import(SysCommandEvent.ContractName)]
        public Event<SysCommandEvent> SysCommand { private get; set; }

        [Import(SizeEvent.ContractName)]
        public Event<SizeEvent> Size { private get; set; }

        [Import(CloseEvent.ContractName)]
        public Event<CloseEvent> Close { private get; set; }

        private readonly Dx11Component _directX;
        private readonly Configurable<System.Drawing.Size> _windowPadding;

        private bool _trackingMouse;

        public RenderWindow(
            Dx11Component directX,
            Configurable<System.Drawing.Size> windowPadding)
        {
            _directX = directX;
            _windowPadding = windowPadding;
        }

        [Import(EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> EmulateMessage 
        {
            set => value.Subscribe((ref EmulateMessageEvent message) =>
            {
                message.Result = User32Methods.SendMessage(
                    Handle, 
                    (uint)message.Message.Id, 
                    message.Message.WParam, 
                    message.Message.WParam);
            });
        }

        protected override void OnMessage(ref WindowMessage msg)
        {
            //Console.WriteLine(msg.Id);
            base.OnMessage(ref msg);
        }

        protected override void OnSysCommand(ref SysCommandPacket packet)
        {
            var evt = new SysCommandEvent(packet);
            SysCommand.Publish(ref evt);
            if (!evt.IsHandled)
                base.OnSysCommand(ref packet);
        }

        protected override void OnSize(ref SizePacket packet)
        {
            _directX.Resize(packet.Size);

            var evt = new SizeEvent(packet);
            Size.Publish(ref evt);
            if (!evt.IsHandled)
                base.OnSize(ref packet);
        }
        
        protected override void OnClose(ref Packet packet)
        {
            var evt = new CloseEvent();
            Close.Publish(ref evt);
            if (!evt.IsHandled)
                base.OnClose(ref packet);
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            _directX.Initialize(Handle, GetClientSize());
            RedrawFrame();

            base.OnCreate(ref packet);
        }

        protected override void OnPaint(ref PaintPacket packet)
        {
            RaiseLayout();
            Render.Publish(new RenderEvent());
            Validate();
        }

        private void RaiseLayout()
        {
            var size = GetClientSize();
            var rectangle = new RectangleF(0, 0, size.Width, size.Height);

            if (User32Methods.GetWindowPlacement(Handle, out var placement) &&
                placement.ShowCmd == WinApi.User32.ShowWindowCommands.SW_MAXIMIZE)
            {
                rectangle.Left += _windowPadding.Value.Width * 2;
                rectangle.Top += _windowPadding.Value.Height * 2;
                rectangle.Width -= _windowPadding.Value.Width * 2;
                rectangle.Height -= _windowPadding.Value.Height * 2;
            }

            Layout.Publish(new LayoutEvent(rectangle));
        }

        protected override void OnNcCalcSize(ref NcCalcSizePacket packet)
        {
            // Extend the client area into the frame.
            if (packet.ShouldCalcValidRects)
                packet.Result = WindowViewRegionFlags.WVR_DEFAULT;
            else
                base.OnNcCalcSize(ref packet);
        }

        private void RaiseHitTest()
        {
            User32Methods.GetCursorPos(out var mousePosition);
            var position = this.GetWindowRect();
            var result = RaiseHitTest(new Point(
                mousePosition.X - position.Left,
                mousePosition.Y - position.Top
            ));
        }

        public HitTestEvent RaiseHitTest(Point windowRelative)
        {
            var result = new HitTestEvent(windowRelative);
            HitTest.Publish(ref result);
            return result;
        }

        protected override void OnNcHitTest(ref NcHitTestPacket packet)
        {
            if (!_trackingMouse)
            {
                // Set up notification for mouse enter/leave.

                var options = new TrackMouseEventOptions()
                {
                    Size = (uint)Marshal.SizeOf<TrackMouseEventOptions>(),
                    TrackedHwnd = Handle,
                    Flags = TrackMouseEventFlags.TME_LEAVE
                };
                _trackingMouse = User32Methods.TrackMouseEvent(ref options);
            }

            var position = this.GetWindowRect();
            var result = RaiseHitTest(new Point(
                packet.Point.X - position.Left,
                packet.Point.Y - position.Top
            ));

            if (result.IsInBounds)
            {
                // HACK: Windows doesn't handle this too well.
                switch (result.Region)
                {
                    case WinApi.User32.HitTestResult.HTCLOSE:
                    case WinApi.User32.HitTestResult.HTMAXBUTTON:
                    case WinApi.User32.HitTestResult.HTMINBUTTON:
                        packet.Result = WinApi.User32.HitTestResult.HTCLIENT;
                        break;
                    default:
                        packet.Result = result.Region;
                        break;
                }
            }
            else
            {
                packet.Result = WinApi.User32.HitTestResult.HTCLIENT;
            }
        }

        protected override void OnMouseLeave(ref Packet packet)
        {
            // Ensure that nothing thinks that the mouse is in the window.
            _trackingMouse = false;
            RaiseHitTest();
        }

        protected override void OnMouseButton(ref MouseButtonPacket packet)
        {
            var evt = new MouseButtonEvent(packet);
            MouseButton.Publish(ref evt);
            if (!evt.IsHandled)
                base.OnMouseButton(ref packet);
        }
    }
}
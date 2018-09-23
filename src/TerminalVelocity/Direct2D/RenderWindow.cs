using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Mathematics.Interop;
using WinApi.DxUtils.Component;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public sealed class RenderWindow : EventedWindowCore
    {
        #region Factory
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
                styles: WindowStyles.WS_OVERLAPPEDWINDOW,
                exStyles: WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_NOREDIRECTIONBITMAP);
        }
        #endregion

        public Dx11Component DirectX { get; }
        public WindowPlacement Placement
        {
            get
            {
                User32Methods.GetWindowPlacement(Handle, out var placement);
                return placement;
            }
        }

        private readonly Direct2DRenderer _renderer;
        private HitTestResult _lastHitTestResult;
        private bool _trackingMouse;

        private RenderWindow(Direct2DRenderer renderer)
        {
            DirectX = new Dx11Component();
            _renderer = renderer;
        }

        protected override void OnMessage(ref WindowMessage msg)
        {
            //Console.WriteLine(msg.Id);
            base.OnMessage(ref msg);
        }

        public unsafe void SendSysCommand(SysCommand command)
        {
            var message = new WindowMessage(Handle, (uint)WM.SYSCOMMAND, IntPtr.Zero, IntPtr.Zero);
            var packet = new SysCommandPacket(&message);
            packet.Command = command;
            packet.IsAccelerator = false;
            packet.IsMnemonic = false;
            packet.X = 0;
            packet.X = 0;

            base.OnSysCommand(ref packet);
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            DirectX.Initialize(Handle, GetClientSize());
            RedrawFrame();

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
            Invalidate();
        }

        protected override void OnNcCalcSize(ref NcCalcSizePacket packet)
        {
            // Extend the client area into the frame.
            if (packet.ShouldCalcValidRects)
                packet.Result = WindowViewRegionFlags.WVR_DEFAULT;
            else
                base.OnNcCalcSize(ref packet);
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
            var result = _renderer.Chrome.HitTest(new Point(
                packet.Point.X - position.Left,
                packet.Point.Y - position.Top
            ));
            HandleHitTestResult(result);

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
            
            User32Methods.GetCursorPos(out var mousePosition);
            var position = this.GetWindowRect();
            var result = _renderer.Chrome.HitTest(new Point(
                mousePosition.X - position.Left,
                mousePosition.Y - position.Top
            ));

            HandleHitTestResult(result);
        }

        public void HandleHitTestResult(HitTestResult result)
        {
            _lastHitTestResult = result;
            if (result.Flags.HasFlag(HitTestFlags.Repaint))
                Invalidate();
        }

        protected override void OnMouseButton(ref MouseButtonPacket packet)
        {
            if (!_renderer.Chrome.Event(ref packet))
            {
                base.OnMouseButton(ref packet);
            }
        }

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
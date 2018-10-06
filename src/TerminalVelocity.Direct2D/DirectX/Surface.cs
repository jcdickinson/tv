using System;
using System.Diagnostics;
using System.Drawing;
using SharpDX;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Renderer;

namespace TerminalVelocity.Direct2D.DirectX
{
    public sealed partial class Surface : IDisposable, ISurface
    {
        public static bool Debug { get; set; }

        private delegate ref T Ref<T>();
        private static void DebugSelect<T>(in T debug, in T release, out T result)
#           if DEBUG
            => result = Debug ? debug : release;
#           else
            => result = release;
#           endif

        private readonly CompositionType _compositionType;
        private DirectXRenderEventLoop _eventLoop;
        private D2D _d2d;
        private D3D _d3d;
        private Dxgi _dxgi;
        private Write _write;
        private Compositor _compositor;

        public bool IsInitialized { get; private set; }
        public bool IsDisposing { get; private set; }
        public bool IsDrawing { get; private set; }

        public Surface()
        {
            _d2d = new D2D(GetDxgi);
            _d3d = new D3D(GetDxgi);
            _dxgi = new Dxgi();
            _write = new Write();
            _compositor = new Compositor(GetDxgi);
            _compositionType = GetCompositionType();
        }

        [DebuggerStepThrough]
        private ref Dxgi GetDxgi() => ref _dxgi;

        public void Initialize(
            DirectXRenderEventLoop eventLoop,
            IntPtr hwnd,
            System.Drawing.SizeF size)
        {
            IsDisposing = false;
            _d3d.CreateFactory();
            _d2d.CreateFactory();
            _write.CreateFactory();

            UpdateScale();

            ConvertToPixels(size, out Size2 size2);

            _eventLoop = eventLoop;
            _d3d.Create(hwnd, size2, _compositionType);
            _d2d.Create();
            _write.Create();
            if (hwnd != IntPtr.Zero &&
                _compositionType.HasFlag(CompositionType.Composited))
                _compositor.Create(hwnd, _compositionType);
            IsInitialized = true;
        }

        private void CheckThread()
        {
            if (_eventLoop == null || !_eventLoop.IsOnEventLoopThread)
                throw new InvalidOperationException("The current thread is not the render thread.");
            if (!IsInitialized)
                throw new InvalidOperationException("The renderer loop is not ready for requests.");
        }

        public void PrepareDispose() => IsDrawing = IsDisposing = false;

        public void Dispose()
        {
            PrepareDispose();
            _eventLoop = null;
            _write.Dispose();
            _d2d.Dispose();
            _dxgi.Dispose();
            _d3d.Dispose();
        }

        public void Resize(in System.Drawing.SizeF size)
        {
            CheckThread();
            if (IsDisposing) return;

            if (_compositionType.HasFlag(CompositionType.Composited) &&
                (size.Width <= 0 || size.Height <= 0))
                return;

            UpdateScale();

            ConvertToPixels(size, out Size2 size2);

            _d2d.Disconnect();
            _d3d.Disconnect();

            _dxgi.Resize(size2);

            _d3d.Connect();
            _d2d.Connect();
        }

        public void BeginDraw()
        {
            CheckThread();
            if (IsDisposing) return;

            IsDrawing = true;

            UpdateScale();

            _d2d.Context.BeginDraw();
            _d2d.Context.Clear(new RawColor4(0, 0, 0, 0));
            UpdateScale();
        }

        public void UpdateScale()
        {
            if (IsDisposing) return;
            Size2F scale = _d2d.Factory.DesktopDpi;
            _dpiScale = new Size2F(
                scale.Width / 96.0f,
                scale.Height / 96.0f
            );
        }

        public void EndDraw()
        {
            CheckThread();
            if (IsDisposing) return;

            IsDrawing = false;

            _d2d.Context.EndDraw();
            _dxgi.SwapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
        }
    }
}

using System;
using System.Diagnostics;
using NetCoreEx.Geometry;
using SharpDX;
using SharpDX.DXGI;

namespace TerminalVelocity.Direct2D.DirectX
{
    public sealed partial class DirectX : IDisposable
    {
        private delegate ref T Ref<T>();
        private static T DebugSelect<T>(T debug, T release)
#           if DEBUG
            => release;
#           else
            => release;
#           endif

        private readonly DirectCompositionVariant _variant;
        private D2D _d2d;
        private D3D _d3d;
        private Dxgi _dxgi;
        private Write _write;
        private Compositor _compositor;

        public bool IsInitialized { get; private set; }

        public DirectX()
        {
            _d2d = new D2D(GetDxgi);
            _d3d = new D3D(GetDxgi);
            _dxgi = new Dxgi();
            _write = new Write();
            _compositor = new Compositor(GetDxgi);
            _variant = GetDirectCompositionVariant();
        }

        [DebuggerStepThrough]
        private ref Dxgi GetDxgi() => ref _dxgi;

        internal void Initialize(IntPtr hwnd, Size size)
        {
            _d3d.Create(hwnd, size, _variant);
            _d2d.Create();
            _write.Create();
            if (_variant.HasFlag(DirectCompositionVariant.Composited))
                _compositor.Create(hwnd, _variant);
            IsInitialized = true;
        }

        public void Dispose()
        {
            _write.Dispose();
            _d2d.Dispose();
            _dxgi.Dispose();
            _d3d.Dispose();
        }

        internal void Resize(Size size)
        {
            if (_variant.HasFlag(DirectCompositionVariant.Composited) &&
                (size.Width <= 0 || size.Height <= 0))
                return;

            _d2d.Disconnect();
            _d3d.Disconnect();
            _dxgi.Resize(size);
            _d3d.Connect();
            _d2d.Connect();
        }

        internal void BeginDraw()
        {
            _d2d.Context.BeginDraw();
        }

        internal void Clear(Color4 color4)
        {
            _d2d.Context.Clear(color4);
        }

        internal void EndDraw()
        {
            _d2d.Context.EndDraw();
        }

        internal void Present()
        {
            _dxgi.SwapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
        }
    }
}

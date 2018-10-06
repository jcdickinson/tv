using System;
using System.Diagnostics;
using System.Threading;
using SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class DirectX
    {
        private struct D2D : IDisposable
        {
            public readonly Ref<Dxgi> Dxgi;
            public Factory1 Factory;
            public Device Device;
            public DeviceContext Context;

            public D2D(Ref<Dxgi> dxgi)
            {
                Dxgi = dxgi;
                Factory = default;
                Device = default;
                Context = default;
            }

            public void Create()
            {
                if (Device != null) Dispose();

                var creationOptions = new CreationProperties()
                {
                    DebugLevel = DebugSelect(DebugLevel.Warning, DebugLevel.Error),
                    Options = DeviceContextOptions.EnableMultithreadedOptimizations,
                    ThreadingMode = ThreadingMode.SingleThreaded
                };

                Factory = new Factory1(FactoryType.SingleThreaded, creationOptions.DebugLevel);
                Device = new Device(Dxgi().Device, creationOptions);
                Context = new DeviceContext(Device, creationOptions.Options);

                Connect();
            }

            public void Connect()
            {
                using (DXGI.Surface surface = Dxgi().SwapChain.GetBackBuffer<DXGI.Surface>(0))
                using (var bitmap = new Bitmap1(Context, surface))
                {
                    Context.Target = bitmap;
                }
            }

            public void Disconnect()
            {
                Image target = Context?.Target;
                if (target != null)
                {
                    Context.Target = null;
                    target.Dispose();
                }
            }

            public void Dispose()
            {
                Disconnect();

                DisposableHelpers.Dispose(ref Context);
                DisposableHelpers.Dispose(ref Device);
                DisposableHelpers.Dispose(ref Factory);
            }
        }
    }
}

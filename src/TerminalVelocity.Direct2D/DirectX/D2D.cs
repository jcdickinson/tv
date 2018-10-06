using System;
using SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private struct D2D : IDisposable
        {
            public readonly Ref<Dxgi> Dxgi;
            public Factory1 Factory;
            public Device Device;
            public DeviceContext Context;
            private CreationProperties CreationProperties;

            public D2D(Ref<Dxgi> dxgi)
            {
                Dxgi = dxgi;
                Factory = default;
                Device = default;
                Context = default;

                CreationProperties = new CreationProperties()
                {
                    Options = DeviceContextOptions.EnableMultithreadedOptimizations,
                    ThreadingMode = ThreadingMode.SingleThreaded
                };
                DebugSelect(DebugLevel.Warning, DebugLevel.Error, out CreationProperties.DebugLevel);
            }

            public void CreateFactory()
            {
                if (Factory != null) Dispose();

                Factory = new Factory1(FactoryType.SingleThreaded, CreationProperties.DebugLevel);
            }

            public void Create()
            {
                Device = new Device(Dxgi().Device, CreationProperties);
                Context = new DeviceContext(Device, CreationProperties.Options);

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

                Disposable.Dispose(ref Context);
                Disposable.Dispose(ref Device);
                Disposable.Dispose(ref Factory);
            }
        }
    }
}

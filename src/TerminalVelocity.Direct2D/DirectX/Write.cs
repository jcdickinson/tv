using System;
using SharpDX.DirectWrite;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private struct Write : IDisposable
        {
            public Factory Factory;

            public void CreateFactory()
            {
                if (Factory != null) Dispose();

                Factory = new Factory(FactoryType.Shared);
            }

            public void Dispose()
                => Disposable.Dispose(ref Factory);

            public void Create() { }
        }
    }
}

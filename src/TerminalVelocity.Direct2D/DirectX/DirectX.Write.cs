using System;
using SharpDX.DirectWrite;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class DirectX
    {
        private struct Write : IDisposable
        {
            public Factory Factory;

            public void Create()
            {
                if (Factory != null) Dispose();

                Factory = new Factory(FactoryType.Shared);
            }

            public void Dispose()
                => DisposableHelpers.Dispose(ref Factory);
        }
    }
}

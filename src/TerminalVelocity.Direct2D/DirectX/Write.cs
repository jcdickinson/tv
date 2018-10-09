/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

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

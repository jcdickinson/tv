using System;
using System.Threading;

namespace TerminalVelocity.Direct2D.DirectX
{
    internal static class DisposableHelpers
    {
        public static void Dispose<T>(ref T disposable)
            where T : class, IDisposable
            => Interlocked.Exchange(ref disposable, null)?.Dispose();
    }
}

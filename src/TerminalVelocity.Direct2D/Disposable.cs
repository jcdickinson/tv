using System;
using System.Threading;

namespace TerminalVelocity.Direct2D
{
    internal static class Disposable
    {
        public static void Dispose<T>(ref T disposable)
            where T : class, IDisposable
            => Interlocked.Exchange(ref disposable, null)?.Dispose();
    }
}

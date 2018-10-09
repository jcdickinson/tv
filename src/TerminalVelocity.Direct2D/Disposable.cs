/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

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

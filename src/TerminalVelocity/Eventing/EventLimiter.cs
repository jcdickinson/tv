/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Threading;

namespace TerminalVelocity.Eventing
{
    public struct EventLimiter<T>
    {
        private long _latestEventId;
        private long _currentEventId;

        public bool EventPublished<TActual>(ulong eventId)
        {
            if (typeof(T) == typeof(TActual))
            {
                Interlocked.Exchange(ref _latestEventId, (long)eventId);
                return true;
            }
            return false;
        }

        public void FreezeLatest()
            => Interlocked.Exchange(ref _currentEventId, _latestEventId);

        public bool ShouldExecuteEvent<TActual>(ulong eventId, in TActual actual, out T expected)
        {
            if (Interlocked.Read(ref _currentEventId) == (long)eventId &&
                actual is T)
            {
                expected = (T)(object)actual;
                return true;
            }
            expected = default;
            return false;
        }
    }
}

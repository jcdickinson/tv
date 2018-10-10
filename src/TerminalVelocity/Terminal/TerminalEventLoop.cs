/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Threading;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal
{
    public sealed class TerminalEventLoop : EventLoop
    {
        public override int Priority => int.MaxValue / 2;

        private readonly AutoResetEvent _eventReceived;
        private readonly Thread _terminalThread;

        public TerminalEventLoop()
        {
            _eventReceived = new AutoResetEvent(false);
            _terminalThread = new Thread(EventLoop)
            {
                Name = "Terminal Event Loop",
                IsBackground = true
            };
        }

        protected internal override bool OnEventExecuting<T>(ulong eventId, ref T e)
        {
            return base.OnEventExecuting(eventId, ref e);
        }

        protected internal override void OnEventPublished<T>(ulong eventId, in T e) => _eventReceived.Set();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_eventReceived)
                {
                    _terminalThread.Join();
                }
            }
        }

        public override void Execute() => _terminalThread.Start();

        private void EventLoop()
        {
            while (IsRunning)
            {
                _eventReceived.WaitOne();
                ExecuteEvents();
            }
        }
    }
}

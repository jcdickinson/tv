using System.Threading;
using TerminalVelocity.Eventing;

namespace TerminalVelocity
{
    public sealed class InteractionEventLoop : EventLoop
    {
        public override int Priority => int.MaxValue / 4;

        private readonly AutoResetEvent _eventReceived;
        private readonly Thread _interactionThread;

        public InteractionEventLoop()
        {
            _eventReceived = new AutoResetEvent(false);
            _interactionThread = new Thread(EventLoop)
            {
                Name = "Interaction Event Loop",
                IsBackground = true
            };
        }

        protected override void OnEventPublished<T>(ulong eventId, in T e) => _eventReceived.Set();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_eventReceived)
                {
                    _interactionThread.Join();
                }
            }
        }

        public override void Execute() => _interactionThread.Start();

        private void EventLoop()
        {
            CreateSynchronizationContext();

            while (IsRunning)
            {
                _eventReceived.WaitOne();
                ExecuteEvents();
            }
        }
    }
}

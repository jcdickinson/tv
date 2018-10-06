using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalVelocity.Eventing
{
    internal sealed class EventLoopSynchronizationContext : SynchronizationContext, IEvent
    {
        private readonly struct SynchronizationEventData : IDisposable
        {
            public readonly ulong Id;
            public readonly SendOrPostCallback Callback;
            public readonly object State;
            public readonly ManualResetEventSlim WaitHandle;

            public SynchronizationEventData(ulong id, SendOrPostCallback callback, object state, bool createWaitHandle)
            {
                Id = id;
                Callback = callback;
                State = state;
                WaitHandle = createWaitHandle ? new ManualResetEventSlim(false) : null;
            }

            public void Invoke()
            {
                Callback?.Invoke(State);
                WaitHandle?.Set();
            }

            public void Dispose() => WaitHandle?.Dispose();
        }

        private readonly EventLoop _eventLoop;
        private readonly ConcurrentQueue<SynchronizationEventData> _events;
        private long _idFactory;
        private readonly Thread _thread;

        public EventLoopSynchronizationContext(EventLoop eventLoop)
            : this(eventLoop, Thread.CurrentThread)
        {

        }

        private EventLoopSynchronizationContext(EventLoop eventLoop, Thread thread)
        {
            _thread = thread;
            _eventLoop = eventLoop;
            _events = new ConcurrentQueue<SynchronizationEventData>();
        }

        EventStatus IEvent.PublishEvent(ulong eventId)
        {
            while (_events.TryPeek(out SynchronizationEventData publication) && publication.Id <= eventId)
            {
                // Only occurs on one thread.
                _events.TryDequeue(out publication);
                publication.Invoke();
            }
            return EventStatus.Continue;
        }

        private void PostOrSend(SendOrPostCallback d, object state, bool wait)
        {
            if (Thread.CurrentThread == _thread)
            {
                d(state);
                return;
            }

            var id = (ulong)Interlocked.Increment(ref _idFactory);
            using (var e = new SynchronizationEventData(id, d, state, wait))
            {
                _events.Enqueue(e);
                _eventLoop.Publish(id, this, e);
                e.WaitHandle?.Wait();
            }
        }

        public override void Post(SendOrPostCallback d, object state)
            => PostOrSend(d, state, false);

        public override void Send(SendOrPostCallback d, object state)
            => PostOrSend(d, state, true);
    }
}

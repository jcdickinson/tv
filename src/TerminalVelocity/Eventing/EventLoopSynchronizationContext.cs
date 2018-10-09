/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TerminalVelocity.Eventing
{
    internal sealed class EventLoopSynchronizationContext : SynchronizationContext, IEvent
    {
        private readonly struct SynchronizationEventData : IDisposable
        {
            public readonly EventLoop EventLoop;
            public readonly ulong Id;
            public readonly SendOrPostCallback Callback;
            public readonly object State;
            public readonly ManualResetEventSlim WaitHandle;

            public SynchronizationEventData(EventLoop eventLoop, ulong id, SendOrPostCallback callback, object state, bool createWaitHandle)
            {
                EventLoop = eventLoop;
                Id = id;
                Callback = callback;
                State = state;
                WaitHandle = createWaitHandle ? new ManualResetEventSlim(false) : null;
            }

            public void Invoke()
            {
                SynchronizationEventData tmp = this;
                EventLoop.OnEventExecuting(Id, ref tmp);
                Callback?.Invoke(State);
                EventLoop.OnEventExecuted(Id, tmp, EventStatus.Continue);
                WaitHandle?.Set();
            }

            public void Dispose() => WaitHandle?.Dispose();
        }

        private readonly EventLoop _eventLoop;
        private readonly ConcurrentQueue<SynchronizationEventData> _events;
        private long _idFactory;

        public int ManagedThreadId { get; }

        public EventLoopSynchronizationContext(EventLoop eventLoop)
            : this(eventLoop, Thread.CurrentThread.ManagedThreadId)
        {

        }

        private EventLoopSynchronizationContext(EventLoop eventLoop, int managedThreadId)
        {
            ManagedThreadId = managedThreadId;
            _eventLoop = eventLoop;
            _events = new ConcurrentQueue<SynchronizationEventData>();
        }

        EventStatus IEvent.PublishEvent(ulong eventId)
        {
            while (
                _events.TryPeek(out SynchronizationEventData publication) &&
                publication.Id <= eventId)
            {
                // Only occurs on one thread.
                _events.TryDequeue(out publication);
                publication.Invoke();
            }
            return EventStatus.Continue;
        }

        private void PostOrSend(SendOrPostCallback d, object state, bool wait)
        {
            var id = (ulong)Interlocked.Increment(ref _idFactory);
            var isCurrentThread = _eventLoop.IsOnEventLoopThread;
     
            using (var e = new SynchronizationEventData(_eventLoop, id, d, state, wait && !isCurrentThread))
            {
                SynchronizationEventData tmp = e;
                _eventLoop.OnEventPublishing(id, ref tmp);

                if (wait && isCurrentThread)
                {
                    _eventLoop.OnEventPublished(id, tmp);
                    _eventLoop.OnEventExecuting(id, ref tmp);
                    d(state);
                    _eventLoop.OnEventExecuted(id, tmp, EventStatus.Continue);
                }
                else
                {
                    _events.Enqueue(e);
                    _eventLoop.Publish(id, this, e);
                    e.WaitHandle?.Wait();
                }
            }
        }

        public override void Post(SendOrPostCallback d, object state)
            => PostOrSend(d, state, false);

        public override void Send(SendOrPostCallback d, object state)
            => PostOrSend(d, state, true);
    }
}

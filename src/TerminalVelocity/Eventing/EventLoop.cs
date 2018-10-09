using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TerminalVelocity.Eventing
{
    public abstract class EventLoop : IDisposable
    {
        protected readonly struct DisposeEvent
        {

        }

        private readonly struct EventPublication
        {
            public readonly ulong Id;
            public readonly IEvent Event;

            public EventPublication(ulong id, IEvent @event)
            {
                Id = @id;
                Event = @event;
            }
        }

        private readonly ConcurrentQueue<EventPublication> _events;
        private readonly CancellationTokenSource _cancellationToken;
        private EventLoopSynchronizationContext _synchronizationContext;

        public abstract int Priority { get; }
        protected bool IsRunning => !_cancellationToken.IsCancellationRequested;
        protected CancellationToken CancellationToken => _cancellationToken.Token;
        protected SynchronizationContext SynchronizationContext => _synchronizationContext;

        public bool IsOnEventLoopThread => _synchronizationContext == null
            ? false
            : Thread.CurrentThread.ManagedThreadId == _synchronizationContext.ManagedThreadId;

        public EventLoop()
        {
            _events = new ConcurrentQueue<EventPublication>();
            _cancellationToken = new CancellationTokenSource();
        }

        protected void CreateSynchronizationContext()
        {
            _synchronizationContext = new EventLoopSynchronizationContext(this);
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        public abstract void Execute();

        public void Dispose()
        {
            // Make sure finally runs.
            using (_cancellationToken)
            {
                var dispose = new DisposeEvent();
                OnEventPublishing(0, ref dispose);
                _cancellationToken.Cancel();
                OnEventPublished(0, dispose);
                OnEventExecuting(0, ref dispose);
                Dispose(true);
                OnEventExecuted(0, dispose, EventStatus.Halt);
            }
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        internal void Publish<TEvent, TPayload>(ulong id, TEvent @event, TPayload e)
            where TEvent : IEvent
            where TPayload : struct
        {
            _events.Enqueue(new EventPublication(id, @event));
            OnEventPublished(id, e);
        }

        protected internal virtual bool OnEventPublishing<T>(ulong eventId, ref T e)
            where T : struct
            => true;

        protected internal abstract void OnEventPublished<T>(ulong eventId, in T e)
            where T : struct;

        protected internal virtual void OnEventExecuting<T>(ulong eventId, ref T e)
            where T : struct
        { }

        protected internal virtual void OnEventExecuted<T>(ulong eventId, in T e, EventStatus eventStatus)
            where T : struct
        { }

        protected void ExecuteEvents()
        {
            while (_events.TryDequeue(out EventPublication @event))
                @event.Event.PublishEvent(@event.Id);
        }
    }
}

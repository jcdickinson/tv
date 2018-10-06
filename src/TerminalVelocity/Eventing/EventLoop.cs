using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TerminalVelocity.Eventing
{
    public abstract class EventLoop : IDisposable
    {
        protected sealed class DisposeEvent
        {
            internal static readonly DisposeEvent Instance = new DisposeEvent();
            private DisposeEvent() { }
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
        protected SynchronizationContext SynchronizationContext { get; }
        public abstract int Priority { get; }

        protected bool IsRunning => !_cancellationToken.IsCancellationRequested;
        protected CancellationToken CancellationToken => _cancellationToken.Token;

        public EventLoop()
        {
            _events = new ConcurrentQueue<EventPublication>();
            _cancellationToken = new CancellationTokenSource();
            SynchronizationContext = new EventLoopSynchronizationContext(this);
        }

        public abstract void Execute();

        public void Dispose()
        {
            // Make sure finally runs.
            using (_cancellationToken)
            {
                _cancellationToken.Cancel();
                OnEventPublished(DisposeEvent.Instance);
                while (_events.TryDequeue(out EventPublication @event))
                {
                    try
                    {
                        @event.Event.PublishEvent(@event.Id);
                    }
                    catch (OperationCanceledException) { }
                }
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        internal void Publish<TEvent, TPayload>(ulong id, TEvent @event, TPayload e)
            where TEvent : IEvent
        {
            _events.Enqueue(new EventPublication(id, @event));
            OnEventPublished(e);
        }

        protected abstract void OnEventPublished<T>(T e);

        protected void ExecuteEvents()
        {
            while (_events.TryDequeue(out EventPublication @event))
                @event.Event.PublishEvent(@event.Id);
        }
    }
}

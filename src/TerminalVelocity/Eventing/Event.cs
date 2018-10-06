using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using TerminalVelocity.Collections.Concurrent;

namespace TerminalVelocity.Eventing
{
    public abstract class Event<TEventLoop, TEvent> : IEvent, IDisposable
        where TEventLoop : EventLoop
        where TEvent : struct
    {
        private sealed class Subscriber : IDisposable
        {
            public ConcurrentLinkedList<Subscriber>.SingleLinkNode Node { get; }
            public EventSubscriber<TEvent> Handler { get; }
            private readonly Event<TEventLoop, TEvent> _eventLoop;
            
            public Subscriber(EventSubscriber<TEvent> handler, Event<TEventLoop, TEvent> eventLoop)
            {
                Handler = handler;
                _eventLoop = eventLoop;
                Node = new ConcurrentLinkedList<Subscriber>.SingleLinkNode(this);
            }

            public void Dispose() => _eventLoop.Unsubscribe(Node);
        }

        private readonly struct EventPublication
        {
            public readonly ulong Id;
            public readonly TEvent Data;

            public EventPublication(ulong id, TEvent data)
            {
                Id = id;
                Data = data;
            }
        }

        private readonly struct EventWrapper
        {
            public readonly Action<TEvent> Subscriber;

            [DebuggerStepThrough]
            public EventWrapper(Action<TEvent> subscriber)
                => Subscriber = subscriber;

            [DebuggerStepThrough]
            public EventStatus Invoke(in TEvent e)
            {
                Subscriber?.Invoke(e);
                return EventStatus.Continue;
            }
        }

        public event EventSubscriber<TEvent> Raised
        {
            add => Subscribe(value);
            remove => throw new NotSupportedException();
        }

        private readonly TEventLoop _eventLoop;
        private readonly ConcurrentQueue<EventPublication> _events;
        private readonly ConcurrentLinkedList<Subscriber> _subscribers;
        public string Name { get; }
        private long _idFactory = 1;

        private Event()
        {
            _events = new ConcurrentQueue<EventPublication>();
            _subscribers = new ConcurrentLinkedList<Subscriber>();

            Name = GetType().FullName;
        }

        public Event(TEventLoop eventLoop)
            : this()
            => _eventLoop = eventLoop ?? throw new ArgumentNullException(nameof(eventLoop));
        
        public Event(EventSubscriber<TEvent> handler)
            : this()
            => Subscribe(handler);

        public Event(Action<TEvent> handler)
            : this()
            => Subscribe(new EventWrapper(handler).Invoke);

        public void Publish(TEvent data)
        {
            var id = (ulong)Interlocked.Increment(ref _idFactory);
            if (_eventLoop == null)
            {
                Publish(data);
            }
            else if (_eventLoop.OnEventPublishing((ulong)id, ref data))
            {
                _events.Enqueue(new EventPublication(id, data));
                _eventLoop.Publish(id, this, data);
            }
        }

        public void Dispose() => _subscribers.Clear();

        private void Unsubscribe(ConcurrentLinkedList<Subscriber>.SingleLinkNode node)
            => _subscribers.Remove(node);

        public IDisposable Subscribe(EventSubscriber<TEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var subscriber = new Subscriber(handler, this);
            _subscribers.AddFirst(subscriber.Node);
            return subscriber;
        }

        EventStatus IEvent.PublishEvent(ulong eventId)
        {
            while (_events.TryPeek(out EventPublication publication) && publication.Id <= eventId)
            {
                // Only occurs on one thread.
                _events.TryDequeue(out publication);

                //Debug.WriteLine(publication.Data.ToString(), Name);
                TEvent data = publication.Data;
                _eventLoop?.OnEventExecuting(publication.Id, ref data);
                EventStatus status = PublishEvent(publication.Data);
                _eventLoop?.OnEventExecuted(publication.Id, data, status);

                if (status == EventStatus.Halt)
                    return EventStatus.Halt;
            }

            return EventStatus.Continue;
        }

        private EventStatus PublishEvent(in TEvent e)
        {
            foreach (Subscriber subscriber in _subscribers)
            {
                if (subscriber.Handler(e) == EventStatus.Halt)
                    return EventStatus.Halt;
            }
            return EventStatus.Continue;
        }

        public sealed override bool Equals(object obj) => base.Equals(obj);

        public sealed override int GetHashCode() => base.GetHashCode();

        public sealed override string ToString() => Name;
    }
}

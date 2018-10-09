using System;
using System.Threading;
using Xunit;

namespace TerminalVelocity.Eventing
{
    public class TestingEventLoop : EventLoop
    {
        private sealed class SynchronizationContextSetter : IDisposable
        {
            private readonly SynchronizationContext _previous;

            public SynchronizationContextSetter(SynchronizationContext previous)
            {
                _previous = previous;
            }

            public void Dispose() => SynchronizationContext.SetSynchronizationContext(_previous);
        }

        public Action<ulong, object, EventStatus> EventExecuted;
        public Action<ulong, object> EventExecuting;
        public Func<ulong, object, bool> EventPublishing;
        public Action<ulong, object> EventPublished;

        public override int Priority => 0;

        public IDisposable SetSynchronizationContext()
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            CreateSynchronizationContext();
            return new SynchronizationContextSetter(previous);
        }

        public override void Execute()
        {
            ExecuteEvents();
        }

        public void IsDisposeEvent(object o) => Assert.IsType<DisposeEvent>(o);

        protected internal override void OnEventExecuted<T>(ulong eventId, in T e, EventStatus eventStatus)
        {
            EventExecuted?.Invoke(eventId, e, eventStatus);
            base.OnEventExecuted(eventId, e, eventStatus);
        }

        protected internal override void OnEventExecuting<T>(ulong eventId, ref T e)
        {
            EventExecuting?.Invoke(eventId, e);
            base.OnEventExecuting(eventId, ref e);
        }

        protected internal override bool OnEventPublishing<T>(ulong eventId, ref T e)
        {
            if (!(EventPublishing?.Invoke(eventId, e) ?? true))
                return false;
            return base.OnEventPublishing(eventId, ref e);
        }

        protected internal override void OnEventPublished<T>(ulong eventId, in T e)
        {
            EventPublished?.Invoke(eventId, e);
        }
    }
}

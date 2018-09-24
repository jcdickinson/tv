using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Threading;

namespace TerminalVelocity
{
    [Shared]
    public sealed class Event<T>
    {
        private readonly struct SubscriberInfo
        {
            public readonly int Cookie;
            public readonly EventHandler Handler;

            public SubscriberInfo(int cookie, EventHandler handler)
            {
                Cookie = cookie;
                Handler = handler;
            }
        }

        public delegate void EventHandler(ref T payload);

        private volatile int _cookie;
        private readonly LinkedList<SubscriberInfo> _targets;

        public Event()
        {
            _targets = new LinkedList<SubscriberInfo>();
        }

        public int Subscribe(EventHandler handler)
        {
            var cookie = Interlocked.Increment(ref _cookie);
            var subscriber = new SubscriberInfo(cookie, handler);
            lock (_targets) _targets.AddLast(subscriber);
            return cookie;
        }

        public void Unsubscribe(int cookie)
        {
            lock(_targets)
            {
                for (var item = _targets.First; item.Next != null; item = item.Next) 
                {
                    if (item.Value.Cookie == cookie)
                    {
                        _targets.Remove(item);
                        break;
                    }
                }
            }
        }

        public void Publish(T payload) => Publish(ref payload);

        public void Publish(ref T payload)
        {
            lock(_targets)
            {
                for (var item = _targets.First; item != null; item = item.Next) 
                {
                    item.Value.Handler(ref payload);
                }
            }
        }
    }
}
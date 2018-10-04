using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Threading;

namespace TerminalVelocity
{
    [Shared]
    public sealed class Event<T>
    {
        private readonly struct SubscriberInfo
        {
            public readonly int Cookie;
            public readonly CancellingEventHandler Handler;

            public SubscriberInfo(int cookie, CancellingEventHandler handler)
            {
                Cookie = cookie;
                Handler = handler;
            }
        }

        public delegate void EventHandler(ref T payload);
        public delegate bool CancellingEventHandler(ref T payload);

        private volatile int _cookie;
        private readonly LinkedList<SubscriberInfo> _targets;
        private readonly string _name;

        public Event(string name)
        {
            _name = name;
            _targets = new LinkedList<SubscriberInfo>();
        }

        public Event(string name, Action<T> handler)
            : this(name)
        {
            Subscribe((ref T x) => handler(x));
        }

        public Event(string name, Func<T, T> handler)
            : this(name)
        {
            Subscribe((ref T x) => x = handler(x));
        }
        
        public int Subscribe(EventHandler handler) => Subscribe((ref T evt) =>
        {
            handler(ref evt);
            return false;
        });

        public int Subscribe(CancellingEventHandler handler)
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

        public bool Publish(T payload) => Publish(ref payload);

        public bool Publish(ref T payload)
        {
            lock(_targets)
            {
                Debug.WriteLine(payload.ToString(), _name);
                for (var item = _targets.First; item != null; item = item.Next) 
                {
                    if (item.Value.Handler(ref payload))
                        return true;
                }
            }

            return false;
        }
    }
}
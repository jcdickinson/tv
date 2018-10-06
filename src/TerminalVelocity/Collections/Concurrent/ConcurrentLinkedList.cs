using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace TerminalVelocity.Collections.Concurrent
{
    internal sealed class ConcurrentLinkedList<T> : IEnumerable<T>
    {
        public sealed class SingleLinkNode
        {
            internal bool Removed;
            internal SingleLinkNode Next;
            public T Item { get; }

            public SingleLinkNode(T item) => Item = item;
        }

        public struct Enumerator : IEnumerator<T>
        {
            public T Current => _current == null ? default : _current.Item;

            object IEnumerator.Current => Current;

            private SingleLinkNode _current;

            internal Enumerator(SingleLinkNode head)
                => _current = head;

            public void Dispose() => _current = null;

            public bool MoveNext()
            {
                do
                {
                    _current = _current?.Next;
                } while (_current != null && _current.Removed);
                return _current != null;
            }

            public void Reset() => throw new NotSupportedException();
        }

        private readonly SingleLinkNode _head;

        public ConcurrentLinkedList() =>
            // Head is always present to simplify CAS loops.
            _head = new SingleLinkNode(default);

        public void Clear() => _head.Next = null;

        public void AddFirst(SingleLinkNode node)
        {
            while (true)
            {
                SingleLinkNode next = _head.Next;
                node.Next = next;
                if (Interlocked.CompareExchange(ref _head.Next, node, next) == next)
                {
                    Thread.MemoryBarrier();
                    if (next == null || !next.Removed) return;
                }
            }
        }

        public bool Remove(SingleLinkNode node)
        {
            SingleLinkNode current = _head;
            SingleLinkNode previous = current;

            node.Removed = true;
            Thread.MemoryBarrier();

            while (current != null)
            {
                SingleLinkNode next = current.Next;
                Thread.MemoryBarrier();

                if (current.Removed)
                {
                    if (current != node || previous.Removed)
                    {
                        current = _head;
                        continue;
                    }

                    if (Interlocked.CompareExchange(ref previous.Next, next, current) == next)
                    {
                        Thread.MemoryBarrier();
                        if (previous.Removed)
                        {
                            current = _head;
                            continue;
                        }
                        return true;
                    }
                }

                previous = current;
                current = next;
            }

            return false;
        }

        public Enumerator GetEnumerator() => new Enumerator(_head);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

using System.Threading;

namespace TerminalVelocity.Collections.Concurrent
{
    public sealed class SingleConcurrentQueue<T>
    {
        private sealed class Box
        {
            public readonly T Value;
            public Box(T value) => Value = value;
        }

        private volatile Box _box;
        public SingleConcurrentQueue() { }

        public void Enqueue(T value) => _box = new Box(value);

        public bool TryEnqueue(T value)
        {
            var box = new Box(value);
            Box old = Interlocked.CompareExchange(ref _box, box, null);
            return old is null;
        }

        public bool TryDequeue(out T value)
        {
            Box box = Interlocked.Exchange(ref _box, null);
            if (box is null)
            {
                value = default;
                return false;
            }
            else
            {
                value = box.Value;
                return true;
            }
        }
    }
}

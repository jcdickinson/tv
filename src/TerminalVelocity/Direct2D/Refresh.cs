using System;
using System.Threading;

namespace TerminalVelocity.Direct2D
{
    internal struct Refresh<T>
    {
        private readonly Func<T> _factory;
        private int _state;
        private T _value;

        public T Value
        {
            get
            {
                if (_state == 0)
                {
                    _state = 1;
                    _value = _factory();
                }
                return _value;
            }
        }

        public Refresh(Func<T> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factory = factory;
            _state = 0;
            _value = default;
        }

        public void Expire(bool dispose = false)
        {
            if (dispose && _value is IDisposable disposable)
                disposable.Dispose();
            _value = default;
        }

        public static implicit operator T (Refresh<T> refresh) => refresh.Value;
    }
}
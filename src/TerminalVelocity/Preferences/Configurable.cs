using System;
using System.Composition;
using System.Threading;

namespace TerminalVelocity.Preferences
{
    [Shared]
    public class Configurable<TValue> : IDisposable
    {
        public event Action ValueReset;

        private TValue _value;
        private Func<TValue> _factory;
        private int _state;

        public TValue Value
        {
            get
            {
                if (_state == -1)
                    throw new ObjectDisposedException("Configurable");
                else if (_state == 0)
                {
                    _value = _factory();
                    _state = 1;
                }
                return _value;
            }
        }

        public bool IsValueCreated => _state > 0;

        public bool IsConstant => _state == 2;

        public bool IsDisposed => _state == -1;

        public Configurable(Func<TValue> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Configurable(TValue constant)
        {
            _value = constant;
            _state = 2;
        }

        public Configurable<TResult> Select<TResult>(Func<TValue, TResult> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return new Configurable<TValue, TResult>(this, selector);
        }

        public Configurable<TResult> Join<TOther, TResult>(Configurable<TOther> other, Func<TValue, TOther, TResult> selector)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Configurable<TValue, TOther, TResult>(this, other, selector);
        }

        public Configurable<(TValue, TOther)> Join<TOther>(Configurable<TOther> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Configurable<TValue, TOther, (TValue, TOther)>(this, other, Zip);
        }

        private static (TValue, TOther) Zip<TOther>(TValue first, TOther second) => (first, second);

        public virtual void Reset()
        {
            if (_state == 1) 
            {
                var oldValue = _value;
                _value = default;

                _state = 0;
                if (oldValue is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public void Dispose()
        {
            if (_state != 0 && _value is IDisposable disposable)
            {
                _value = default;
                _state = -1;
                disposable.Dispose();
            }
        }

        public static implicit operator TValue (Configurable<TValue> value) => value.Value;
        
        public static implicit operator Configurable<TValue> (TValue value) => new Configurable<TValue>(value);
    }

    [Shared]
    internal class Configurable<TValue, TResult> : Configurable<TResult>
    {
        public Configurable(Configurable<TValue> source, Func<TValue, TResult> factory)
            : base(() => factory(source.Value))
        {
            source.ValueReset += Reset;
        }
    }

    [Shared]
    internal class Configurable<TFirst, TSecond, TResult> : Configurable<TResult>
    {
        public Configurable(Configurable<TFirst> first, Configurable<TSecond> second, Func<TFirst, TSecond, TResult> factory)
            : base(() => factory(first.Value, second.Value))
        {
            first.ValueReset += Reset;
            second.ValueReset += Reset;
        }
    }
}
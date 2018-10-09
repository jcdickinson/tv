/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Threading;

namespace TerminalVelocity.Preferences
{
    public class Configurable<TValue> : IDisposable
    {
        public event Action ValueReset;

        private TValue _value;
        private readonly Func<TValue> _factory;
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

        public Configurable(Func<TValue> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

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

        public virtual void Reset() => Reset(0);

        private void Reset(int newState)
        {
            if (_state != -1)
            {
                TValue oldValue = _value;
                _value = default;

                _state = newState;

                Action reset = Interlocked.CompareExchange(ref ValueReset, null, null);
                reset?.Invoke();

                if (oldValue is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public void Dispose()
        {
            if (_state != 0)
                Reset(-1);
        }

        public static implicit operator TValue(Configurable<TValue> value) => value.Value;

        public static implicit operator Configurable<TValue>(TValue value) => new Configurable<TValue>(value);
    }

    internal class Configurable<TValue, TResult> : Configurable<TResult>
    {
        private readonly Configurable<TValue> _source;
        public Configurable(Configurable<TValue> source, Func<TValue, TResult> factory)
            : base(() => factory(source.Value))
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _source.ValueReset += Reset;
        }

        public override void Reset()
        {
            if (_source.IsDisposed)
                base.Dispose();
            else
                base.Reset();
        }
    }

    internal class Configurable<TFirst, TSecond, TResult> : Configurable<TResult>
    {
        private readonly Configurable<TFirst> _first;
        private readonly Configurable<TSecond> _second;

        public Configurable(Configurable<TFirst> first, Configurable<TSecond> second, Func<TFirst, TSecond, TResult> factory)
            : base(() => factory(first.Value, second.Value))
        {
            _first = first ?? throw new ArgumentNullException(nameof(first));
            _second = second ?? throw new ArgumentNullException(nameof(second));
            _first.ValueReset += Reset;
            _second.ValueReset += Reset;
        }

        public override void Reset()
        {
            if (_first.IsDisposed || _second.IsDisposed)
                base.Dispose();
            else
                base.Reset();
        }
    }
}

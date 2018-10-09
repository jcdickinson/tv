/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using Xunit;

namespace TerminalVelocity.Preferences
{
    public static class ConfigurableTests
    {
        private sealed class DisposableSentinel : IDisposable
        {
            public int Value { get; }
            public bool IsDisposed { get; private set; }

            public DisposableSentinel(int value) => Value = value;

            public void Dispose() => IsDisposed = true;
        }

        [Fact, Trait("Category", "Configurable")]
        public static void Configurable_Constant()
        {
            Configurable<int> config = 1337;

            Assert.True(config.IsConstant);
            Assert.True(config.IsValueCreated);
            Assert.False(config.IsDisposed);
            Assert.Equal(1337, config.Value);

            config.Dispose();
            Assert.False(config.IsConstant);
            Assert.False(config.IsValueCreated);
            Assert.True(config.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() => config.Value);
        }

        [Fact, Trait("Category", "Configurable")]
        public static void Configurable_Factory()
        {
            var config = new Configurable<int>(() => 1337);

            Assert.False(config.IsConstant);
            Assert.False(config.IsValueCreated);
            Assert.False(config.IsDisposed);

            Assert.Equal(1337, config.Value);
            Assert.False(config.IsConstant);
            Assert.True(config.IsValueCreated);
            Assert.False(config.IsDisposed);

            config.Dispose();
            Assert.False(config.IsConstant);
            Assert.False(config.IsValueCreated);
            Assert.True(config.IsDisposed);
        }

        [Fact, Trait("Category", "Configurable")]
        public static void Configurable_Disposable()
        {
            var config = new Configurable<DisposableSentinel>(() => new DisposableSentinel(1337));

            Assert.False(config.IsConstant);
            Assert.False(config.IsValueCreated);
            Assert.False(config.IsDisposed);

            DisposableSentinel value = config.Value;
            Assert.Equal(1337, value.Value);
            Assert.False(config.IsConstant);
            Assert.True(config.IsValueCreated);
            Assert.False(config.IsDisposed);

            config.Dispose();
            Assert.False(config.IsConstant);
            Assert.False(config.IsValueCreated);
            Assert.True(config.IsDisposed);
            Assert.True(value.IsDisposed);
        }

        [Fact, Trait("Category", "Configurable")]
        public static void Configurable_Select()
        {
            var i = 1337;
            var config = new Configurable<DisposableSentinel>(() => new DisposableSentinel(i++));
            Configurable<DisposableSentinel> selected = config.Select(x => new DisposableSentinel(x.Value * 2));

            Assert.False(selected.IsConstant);
            Assert.False(selected.IsValueCreated);
            Assert.False(selected.IsDisposed);

            DisposableSentinel value1 = config.Value;
            DisposableSentinel value2 = selected.Value;
            Assert.Equal(1337, value1.Value);
            Assert.Equal(2674, value2.Value);
            Assert.False(selected.IsConstant);
            Assert.True(selected.IsValueCreated);
            Assert.False(selected.IsDisposed);

            config.Reset();
            DisposableSentinel value3 = selected.Value;
            Assert.Equal(2676, value3.Value);

            Assert.True(value1.IsDisposed);
            Assert.True(value2.IsDisposed);

            config.Dispose();
            Assert.True(config.IsDisposed);
            Assert.True(selected.IsDisposed);
            Assert.True(value3.IsDisposed);
        }

        [Fact, Trait("Category", "Configurable")]
        public static void Configurable_Join()
        {
            var i = 1337;
            var config = new Configurable<DisposableSentinel>(() => new DisposableSentinel(i++));
            Configurable<DisposableSentinel> selected = config.Select(x => new DisposableSentinel(x.Value * 2));
            Configurable<Configurable<(int, int)>> join = config.Join(selected, (x, y) => new Configurable<(int, int)>((x.Value, y.Value)));

            Assert.False(selected.IsConstant);
            Assert.False(selected.IsValueCreated);
            Assert.False(selected.IsDisposed);

            DisposableSentinel value1 = config.Value;
            DisposableSentinel value2 = selected.Value;
            Configurable<(int, int)> value3 = join.Value;
            Assert.Equal(1337, value1.Value);
            Assert.Equal(2674, value2.Value);
            Assert.Equal((1337, 2674), value3.Value);
            Assert.False(join.IsConstant);
            Assert.True(join.IsValueCreated);
            Assert.False(join.IsDisposed);

            config.Reset();
            Configurable<(int, int)> value4 = join.Value;
            Assert.Equal((1338, 2676), value4.Value);

            Assert.True(value1.IsDisposed);
            Assert.True(value2.IsDisposed);
            Assert.True(value3.IsDisposed);

            config.Dispose();
            Assert.True(config.IsDisposed);
            Assert.True(selected.IsDisposed);
            Assert.True(join.IsDisposed);
            Assert.True(value4.IsDisposed);
        }
    }
}

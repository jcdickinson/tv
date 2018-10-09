/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using Xunit;

namespace TerminalVelocity.Eventing
{
    public static class EventLimiterTests
    {
        [Fact, Trait("Category", "EventLimiter")]
        public static void EventLimiter_LatestOnly()
        {
            var limiter = new EventLimiter<int>(EventLimiterPolicy.LatestOnly);
            limiter.EventPublished<int>(1);
            limiter.EventPublished<long>(2);
            limiter.EventPublished<int>(2);

            limiter.FreezeLatest();
            limiter.EventPublished<int>(3);

            Assert.False(limiter.ShouldExecuteEvent(1, 123, out int evt));
            Assert.Equal(0, evt);
            Assert.True(limiter.ShouldExecuteEvent(2, 123, out evt));
            Assert.Equal(123, evt);
            Assert.False(limiter.ShouldExecuteEvent(2, 123L, out evt));
            Assert.False(limiter.ShouldExecuteEvent(3, 123, out evt));
            Assert.Equal(0, evt);
        }

        [Fact, Trait("Category", "EventLimiter")]
        public static void EventLimiter_UpToLatest()
        {
            var limiter = new EventLimiter<int>(EventLimiterPolicy.UpToLatest);
            limiter.EventPublished<int>(1);
            limiter.EventPublished<long>(2);
            limiter.EventPublished<int>(2);

            limiter.FreezeLatest();
            limiter.EventPublished<int>(3);

            Assert.True(limiter.ShouldExecuteEvent(1, 123, out int evt));
            Assert.Equal(123, evt);
            Assert.True(limiter.ShouldExecuteEvent(2, 456, out evt));
            Assert.Equal(456, evt);
            Assert.False(limiter.ShouldExecuteEvent(2, 123L, out evt));
            Assert.False(limiter.ShouldExecuteEvent(3, 123, out evt));
            Assert.Equal(0, evt);
        }
    }
}

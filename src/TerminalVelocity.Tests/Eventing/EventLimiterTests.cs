using Xunit;

namespace TerminalVelocity.Eventing
{
    public static class EventLimiterTests
    {
        [Fact, Trait("Category", "EventLimiter")]
        public static void EventLimiter_FreezeLatest()
        {
            var limiter = new EventLimiter<int>();
            limiter.EventPublished<int>(1);
            limiter.EventPublished<long>(2);

            limiter.FreezeLatest();
            limiter.EventPublished<int>(3);

            Assert.True(limiter.ShouldExecuteEvent(1, 123, out int evt));
            Assert.Equal(123, evt);
            Assert.False(limiter.ShouldExecuteEvent(2, 123L, out evt));
            Assert.False(limiter.ShouldExecuteEvent(3, 123, out evt));
            Assert.Equal(0, evt);
        }
    }
}

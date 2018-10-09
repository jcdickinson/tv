using Xunit;

namespace TerminalVelocity.Eventing
{
    public static class EventLoopTests
    {
        [Fact, Trait("Category", "EventLoop")]
        public static void EventLoop_Subscribe_Publish()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var eventDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                    Assert.Equal(EventStatus.Halt, status);
                }
            };

            var ev = new TestingEvent(loop);

            ev.Subscribe((in TestingEventData data) =>
            {
                ++eventDispatched;
                Assert.Equal(123, data.Value);
                return EventStatus.Halt;
            });

            ev.Publish(new TestingEventData(123));

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);
            Assert.Equal(0, eventDispatched);

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
            Assert.Equal(1, eventDispatched);
        }

        [Fact, Trait("Category", "EventLoop")]
        public static void EventLoop_Subscribe_Publish_Prevent()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var eventDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                    return false;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                    Assert.False(true);
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                    Assert.False(true);
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                    Assert.False(true);
                }
            };

            var ev = new TestingEvent(loop);

            ev.Subscribe((in TestingEventData data) =>
            {
                ++eventDispatched;
                Assert.Equal(123, data.Value);
                return EventStatus.Halt;
            });

            ev.Publish(new TestingEventData(123));

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(0, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);
            Assert.Equal(0, eventDispatched);

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(0, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);
            Assert.Equal(0, eventDispatched);
        }

        [Fact, Trait("Category", "EventLoop")]
        public static void EventLoop_Subscribe_Publish_DisposeEvent()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var eventDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                    TestingEventData te = Assert.IsType<TestingEventData>(e);
                    Assert.Equal(123, te.Value);
                }
            };

            var ev = new TestingEvent(loop);

            using (System.IDisposable tmp = ev.Subscribe((in TestingEventData data) =>
            {
                ++eventDispatched;
                Assert.False(true);
                return EventStatus.Continue;
            })) { }

            ev.Publish(new TestingEventData(123));

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);
            Assert.Equal(0, eventDispatched);

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
            Assert.Equal(0, eventDispatched);
        }

        [Fact, Trait("Category", "EventLoop")]
        public static void EventLoop_Dispose()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;

            var loop = new TestingEventLoop();

            loop.EventPublishing = (id, e) =>
            {
                ++publishingDispatched;
                loop.IsDisposeEvent(e);
                return false;
            };
            loop.EventPublished = (id, e) =>
            {
                ++publishedDispatched;
                loop.IsDisposeEvent(e);
            };
            loop.EventExecuting = (id, e) =>
            {
                ++executingDispatched;
                loop.IsDisposeEvent(e);
            };
            loop.EventExecuted = (id, e, status) =>
            {
                ++executedDispatched;
                loop.IsDisposeEvent(e);
                Assert.Equal(EventStatus.Halt, status);
            };

            Assert.Equal(0, publishingDispatched);
            Assert.Equal(0, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);

            using (loop) { }

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
        }
    }
}

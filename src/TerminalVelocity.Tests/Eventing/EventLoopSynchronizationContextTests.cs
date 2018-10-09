using System.Threading;
using Xunit;

namespace TerminalVelocity.Eventing
{
    public static class EventLoopSynchronizationContextTests
    {
        [Fact, Trait("Category", "EventLoopSynchronizationContext")]
        public static void EventLoopSynchronizationContext_Post_SameThread()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var handlerDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                }
            };

            loop.SetSynchronizationContext();
            SynchronizationContext.Current.Post(
                x =>
                {
                    handlerDispatched++;
                    Assert.Equal(123, Assert.IsType<int>(x));
                },
                123
            );

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(0, executingDispatched);
            Assert.Equal(0, executedDispatched);
            Assert.Equal(0, handlerDispatched);

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
            Assert.Equal(1, handlerDispatched);
        }

        [Fact, Trait("Category", "EventLoopSynchronizationContext")]
        public static void EventLoopSynchronizationContext_Post_NewThread()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var handlerDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                }
            };

            loop.SetSynchronizationContext();
            SynchronizationContext ctx = SynchronizationContext.Current;

            using (var mutex1 = new ManualResetEvent(false))
            using (var mutex2 = new ManualResetEvent(false))
            {
                new Thread(() =>
                {
                    if (!mutex1.WaitOne(1000)) return;
                    ctx.Send(
                        x =>
                        {
                            handlerDispatched++;
                            Assert.Equal(123, Assert.IsType<int>(x));
                            mutex2.Set();
                        },
                        123
                    );
                }).Start();

                Assert.Equal(0, publishingDispatched);
                Assert.Equal(0, publishedDispatched);
                Assert.Equal(0, executingDispatched);
                Assert.Equal(0, executedDispatched);
                Assert.Equal(0, handlerDispatched);

                mutex1.Set();
                Assert.False(mutex2.WaitOne(1000));

                Assert.Equal(1, publishingDispatched);
                Assert.Equal(1, publishedDispatched);
                Assert.Equal(0, executingDispatched);
                Assert.Equal(0, executedDispatched);
                Assert.Equal(0, handlerDispatched);

                loop.Execute();
                Assert.True(mutex2.WaitOne(1000));

                Assert.Equal(1, publishingDispatched);
                Assert.Equal(1, publishedDispatched);
                Assert.Equal(1, executingDispatched);
                Assert.Equal(1, executedDispatched);
                Assert.Equal(1, handlerDispatched);
            }
        }

        [Fact, Trait("Category", "EventLoopSynchronizationContext")]
        public static void EventLoopSynchronizationContext_Send_SameThread()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var handlerDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                }
            };

            loop.SetSynchronizationContext();
            SynchronizationContext.Current.Send(
                x =>
                {
                    handlerDispatched++;
                    Assert.Equal(123, Assert.IsType<int>(x));
                },
                123
            );

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
            Assert.Equal(1, handlerDispatched);

            loop.Execute();

            Assert.Equal(1, publishingDispatched);
            Assert.Equal(1, publishedDispatched);
            Assert.Equal(1, executingDispatched);
            Assert.Equal(1, executedDispatched);
            Assert.Equal(1, handlerDispatched);
        }

        [Fact, Trait("Category", "EventLoopSynchronizationContext")]
        public static void EventLoopSynchronizationContext_Send_NewThread()
        {
            var publishingDispatched = 0;
            var publishedDispatched = 0;
            var executingDispatched = 0;
            var executedDispatched = 0;
            var handlerDispatched = 0;

            var loop = new TestingEventLoop
            {
                EventPublishing = (id, e) =>
                {
                    ++publishingDispatched;
                    return true;
                },
                EventPublished = (id, e) =>
                {
                    ++publishedDispatched;
                },
                EventExecuting = (id, e) =>
                {
                    ++executingDispatched;
                },
                EventExecuted = (id, e, status) =>
                {
                    ++executedDispatched;
                }
            };

            loop.SetSynchronizationContext();
            SynchronizationContext ctx = SynchronizationContext.Current;

            using (var mutex1 = new ManualResetEvent(false))
            using (var mutex2 = new ManualResetEvent(false))
            {
                new Thread(() =>
                {
                    if (!mutex1.WaitOne(1000)) return;
                    ctx.Send(
                        x =>
                        {
                            handlerDispatched++;
                            Assert.Equal(123, Assert.IsType<int>(x));
                        },
                        123
                    );
                    mutex2.Set();
                }).Start();

                Assert.Equal(0, publishingDispatched);
                Assert.Equal(0, publishedDispatched);
                Assert.Equal(0, executingDispatched);
                Assert.Equal(0, executedDispatched);
                Assert.Equal(0, handlerDispatched);

                mutex1.Set();
                Assert.False(mutex2.WaitOne(1000));

                Assert.Equal(1, publishingDispatched);
                Assert.Equal(1, publishedDispatched);
                Assert.Equal(0, executingDispatched);
                Assert.Equal(0, executedDispatched);
                Assert.Equal(0, handlerDispatched);

                loop.Execute();

                Assert.Equal(1, publishingDispatched);
                Assert.Equal(1, publishedDispatched);
                Assert.Equal(1, executingDispatched);
                Assert.Equal(1, executedDispatched);
                Assert.Equal(1, handlerDispatched);

                Assert.True(mutex2.WaitOne(1000));

                Assert.Equal(1, publishingDispatched);
                Assert.Equal(1, publishedDispatched);
                Assert.Equal(1, executingDispatched);
                Assert.Equal(1, executedDispatched);
                Assert.Equal(1, handlerDispatched);
            }
        }
    }
}

/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using Xunit;

namespace TerminalVelocity.Eventing
{
    public static class EventTests
    {
        [Fact, Trait("Category", "Event")]
        public static void Event_TestConstructor1_Publish()
        {
            var dispatched = 0;
            var sut = new TestingEvent((in TestingEventData x) =>
            {
                ++dispatched;
                Assert.False(true);
                return EventStatus.Halt;
            });

            sut.Subscribe((in TestingEventData x) =>
            {
                ++dispatched;
                Assert.Equal(123, x.Value);
                return EventStatus.Halt;
            });

            sut.Publish(new TestingEventData(123));
            Assert.Equal(1, dispatched);
        }


        [Fact, Trait("Category", "Event")]
        public static void Event_TestConstructor2_Publish()
        {
            var dispatched = 0;
            var sut = new TestingEvent(x =>
            {
                ++dispatched;
                Assert.Equal(123, x.Value);
            });

            sut.Publish(new TestingEventData(123));
            Assert.Equal(1, dispatched);
        }
    }
}

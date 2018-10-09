/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;

namespace TerminalVelocity.Eventing
{
    public class TestingEvent : Event<TestingEventLoop, TestingEventData>
    {
        public TestingEvent(TestingEventLoop eventLoop) : base(eventLoop) { }

        public TestingEvent(EventSubscriber<TestingEventData> handler) : base(handler) { }

        public TestingEvent(Action<TestingEventData> handler) : base(handler) { }
    }

    public readonly struct TestingEventData
    {
        public readonly int Value;

        public TestingEventData(int value) => Value = value;
    }
}

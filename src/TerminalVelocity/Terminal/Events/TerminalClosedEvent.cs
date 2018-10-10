/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class TerminalClosedEvent : Event<TerminalEventLoop, TerminalClosedEventData>
    {
        public TerminalClosedEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public TerminalClosedEvent(EventSubscriber<TerminalClosedEventData> handler) : base(handler) { }

        public TerminalClosedEvent(Action<TerminalClosedEventData> handler) : base(handler) { }
    }

    public readonly struct TerminalClosedEventData
    {
        public readonly TerminalIdentifier Terminal;

        public TerminalClosedEventData(TerminalIdentifier terminal) => Terminal = terminal;

        public override string ToString() => string.Empty;
    }
}

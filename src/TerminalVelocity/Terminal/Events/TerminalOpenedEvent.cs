/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class TerminalOpenedEvent : Event<TerminalEventLoop, TerminalOpenedEventData>
    {
        public TerminalOpenedEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public TerminalOpenedEvent(EventSubscriber<TerminalOpenedEventData> handler) : base(handler) { }

        public TerminalOpenedEvent(Action<TerminalOpenedEventData> handler) : base(handler) { }
    }

    public readonly struct TerminalOpenedEventData
    {
        public readonly TerminalIdentifier Terminal;

        public TerminalOpenedEventData(TerminalIdentifier terminal) => Terminal = terminal;

        public override string ToString() => string.Empty;
    }
}

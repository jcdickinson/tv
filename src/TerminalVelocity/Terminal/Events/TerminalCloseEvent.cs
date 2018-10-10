/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class TerminalCloseEvent : Event<TerminalEventLoop, TerminalCloseEventData>
    {
        public TerminalCloseEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public TerminalCloseEvent(EventSubscriber<TerminalCloseEventData> handler) : base(handler) { }

        public TerminalCloseEvent(Action<TerminalCloseEventData> handler) : base(handler) { }
    }

    public readonly struct TerminalCloseEventData
    {
        public readonly TerminalIdentifier Terminal;

        public TerminalCloseEventData(TerminalIdentifier terminal) => Terminal = terminal;

        public override string ToString() => string.Empty;
    }
}

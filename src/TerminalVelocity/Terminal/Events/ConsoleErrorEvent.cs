/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class ConsoleErrorEvent : Event<TerminalEventLoop, ConsoleErrorEventData>
    {
        public ConsoleErrorEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public ConsoleErrorEvent(EventSubscriber<ConsoleErrorEventData> handler) : base(handler) { }

        public ConsoleErrorEvent(Action<ConsoleErrorEventData> handler) : base(handler) { }
    }

    public readonly struct ConsoleErrorEventData
    {
        public readonly TerminalIdentifier Terminal;
        public readonly ReadOnlyMemory<byte> Buffer;

        public ConsoleErrorEventData(TerminalIdentifier terminal, ReadOnlyMemory<byte> buffer)
            => (Terminal, Buffer) = (terminal, buffer);

        public override string ToString() => string.Empty;
    }
}

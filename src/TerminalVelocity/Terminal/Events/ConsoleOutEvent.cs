/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class ConsoleOutEvent : Event<TerminalEventLoop, ConsoleOutEventData>
    {
        public ConsoleOutEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public ConsoleOutEvent(EventSubscriber<ConsoleOutEventData> handler) : base(handler) { }

        public ConsoleOutEvent(Action<ConsoleOutEventData> handler) : base(handler) { }
    }

    public readonly struct ConsoleOutEventData
    {
        public readonly TerminalIdentifier Terminal;
        public readonly ReadOnlyMemory<byte> Buffer;

        public ConsoleOutEventData(TerminalIdentifier terminal, ReadOnlyMemory<byte> buffer)
            => (Terminal, Buffer) = (terminal, buffer);

        public override string ToString() => string.Empty;
    }
}

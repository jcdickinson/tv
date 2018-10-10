/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class ConsoleInEvent : Event<TerminalEventLoop, ConsoleInEventData>
    {
        public ConsoleInEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public ConsoleInEvent(EventSubscriber<ConsoleInEventData> handler) : base(handler) { }

        public ConsoleInEvent(Action<ConsoleInEventData> handler) : base(handler) { }
    }

    public readonly struct ConsoleInEventData
    {
        public readonly TerminalIdentifier Terminal;
        public readonly ReadOnlyMemory<byte> Buffer;

        public ConsoleInEventData(TerminalIdentifier terminal, ReadOnlyMemory<byte> buffer)
            => (Terminal, Buffer) = (terminal, buffer);

        public override string ToString() => string.Empty;
    }
}

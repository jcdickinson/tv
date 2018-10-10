/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Terminal.Events
{
    [Event]
    public sealed class TerminalOpenEvent : Event<TerminalEventLoop, TerminalOpenEventData>
    {
        public TerminalOpenEvent(TerminalEventLoop eventLoop) : base(eventLoop) { }

        public TerminalOpenEvent(EventSubscriber<TerminalOpenEventData> handler) : base(handler) { }

        public TerminalOpenEvent(Action<TerminalOpenEventData> handler) : base(handler) { }
    }

    public readonly struct TerminalOpenEventData
    {
        public readonly TerminalIdentifier Terminal;
        public readonly string ApplicationName;
        public readonly string Arguments;
        public readonly string WorkingDirectory;
        public readonly string Environment;

        public TerminalOpenEventData(
            string applicationName,
            string arguments,
            string workingDirectory,
            string environment)
        {
            if (string.IsNullOrEmpty(applicationName)) throw new ArgumentNullException(nameof(applicationName));

            Terminal = TerminalIdentifier.Create();
            ApplicationName = applicationName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
            Environment = environment;
        }

        public override string ToString() => string.Empty;
    }
}

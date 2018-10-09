/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class CloseEvent : Event<InteractionEventLoop, CloseEventData>
    {
        public CloseEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct CloseEventData
    {
        public override string ToString() => string.Empty;
    }
}

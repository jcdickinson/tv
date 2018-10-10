/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Renderer.Events
{
    [Event]
    public sealed class CreateEvent : Event<RenderEventLoop, CreateEventData>
    {
        public CreateEvent(RenderEventLoop eventLoop) : base(eventLoop) { }

        public CreateEvent(EventSubscriber<CreateEventData> handler) : base(handler) { }

        public CreateEvent(Action<CreateEventData> handler) : base(handler) { }
    }

    public readonly struct CreateEventData
    {
        public override string ToString() => string.Empty;
    }
}

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

    public struct CreateEventData
    {
        public override string ToString() => string.Empty;
    }
}

using System;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Renderer.Events
{
    [Event]
    public sealed class RenderEvent : Event<RenderEventLoop, RenderEventData>
    {
        public RenderEvent(RenderEventLoop eventLoop) : base(eventLoop) { }

        public RenderEvent(EventSubscriber<RenderEventData> handler) : base(handler) { }

        public RenderEvent(Action<RenderEventData> handler) : base(handler) { }
    }

    public struct RenderEventData
    {
        public override string ToString() => string.Empty;
    }
}

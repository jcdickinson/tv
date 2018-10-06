using System;
using System.Drawing;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Renderer.Events
{
    [Event]
    public sealed class ResizeEvent : Event<RenderEventLoop, ResizeEventData>
    {
        public ResizeEvent(RenderEventLoop eventLoop) : base(eventLoop) { }

        public ResizeEvent(EventSubscriber<ResizeEventData> handler) : base(handler) { }

        public ResizeEvent(Action<ResizeEventData> handler) : base(handler) { }
    }

    public struct ResizeEventData
    {
        public readonly SizeF Size;

        public ResizeEventData(in SizeF size) => Size = size;

        public override string ToString()
            => FormattableString.Invariant($"{Size}");
    }
}

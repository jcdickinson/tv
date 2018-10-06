using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class RenderEvent : Event<RenderEventLoop, RenderEventData>
    {
        public RenderEvent(RenderEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct RenderEventData
    {
        public override string ToString() => string.Empty;
    }
}

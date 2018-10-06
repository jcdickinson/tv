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

using System;
using NetCoreEx.Geometry;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class CreatedEvent : Event<RenderEventLoop, CreatedEventData>
    {
        public CreatedEvent(RenderEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct CreatedEventData
    {
        public IntPtr Hwnd;
        public Size Size;

        public CreatedEventData(IntPtr hwnd, Size size)
        {
            Hwnd = hwnd;
            Size = size;
        }

        public override string ToString() => FormattableString.Invariant($"{Hwnd} {Size}");
    }
}

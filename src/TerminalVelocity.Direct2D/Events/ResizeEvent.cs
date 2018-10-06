using System;
using NetCoreEx.Geometry;
using TerminalVelocity.Eventing;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class ResizeEvent : Event<RenderEventLoop, ResizeEventData>
    {
        public ResizeEvent(RenderEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct ResizeEventData
    {
        public readonly WindowSizeFlag Flag;
        public readonly Size Size;

        public ResizeEventData(in SizePacket packet)
        {
            Flag = packet.Flag;
            Size = packet.Size;
        }

        public override string ToString()
            => FormattableString.Invariant($"{Flag} {Size}");
    }
}

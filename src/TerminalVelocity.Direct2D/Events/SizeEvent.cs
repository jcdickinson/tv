using System;
using NetCoreEx.Geometry;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    public struct SizeEvent
    {
        public const string ContractName = "Size.Events.Direct2D.TerminalVelocity";

        public readonly WindowSizeFlag Flag;
        public readonly Size Size;
        public bool IsHandled;

        public SizeEvent(in SizePacket packet)
        {
            Flag = packet.Flag;
            Size = packet.Size;
            IsHandled = false;
        }
    }
}
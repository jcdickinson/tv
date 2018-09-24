using System;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    public struct SysCommandEvent
    {
        public const string ContractName = "SysCommand.Events.Direct2D.TerminalVelocity";

        public readonly short X;
        public readonly short Y;
        public readonly SysCommand Command;
        public readonly bool IsAccelerator;
        public readonly bool IsMnemonic;
        public bool IsHandled;

        public SysCommandEvent(in SysCommandPacket packet)
        {
            X = packet.X;
            Y = packet.Y;
            Command = packet.Command;
            IsAccelerator = packet.IsAccelerator;
            IsMnemonic = packet.IsMnemonic;
            IsHandled = false;
        }
    }
}
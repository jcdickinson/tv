using System;
using SharpDX;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    public struct MouseButtonEvent
    {
        public const string ContractName = "MouseButton.Events.Direct2D.TerminalVelocity";

        public readonly Point Point;
        public readonly MouseButton Button;
        public readonly bool IsButtonDown;
        public readonly MouseInputKeyStateFlags InputState;
        public readonly MouseButtonResult Result;

        public MouseButtonEvent(in MouseButtonPacket packet)
        {
            Point = new Point(packet.Point.X, packet.Point.Y);
            Button = packet.Button;
            IsButtonDown = packet.IsButtonDown;
            InputState = packet.InputState;
            Result = packet.Result;
        }

        public override string ToString() => IsButtonDown
            ? FormattableString.Invariant($"{Button} Down {Point} {InputState}")
            : FormattableString.Invariant($"{Button} Up {Point} {InputState}");
    }
}
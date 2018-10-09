/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using SharpDX;
using TerminalVelocity.Eventing;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class MouseButtonEvent : Event<InteractionEventLoop, MouseButtonEventData>
    {
        public MouseButtonEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct MouseButtonEventData
    {
        public readonly Point Point;
        public readonly MouseButton Button;
        public readonly bool IsButtonDown;
        public readonly MouseInputKeyStateFlags InputState;
        public readonly MouseButtonResult Result;

        public MouseButtonEventData(in MouseButtonPacket packet)
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

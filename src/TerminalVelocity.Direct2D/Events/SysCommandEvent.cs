/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using TerminalVelocity.Eventing;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class SysCommandEvent : Event<InteractionEventLoop, SysCommandEventData>
    {
        public SysCommandEvent(InteractionEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct SysCommandEventData
    {
        public readonly short X;
        public readonly short Y;
        public readonly SysCommand Command;
        public readonly bool IsAccelerator;
        public readonly bool IsMnemonic;

        public SysCommandEventData(in SysCommandPacket packet)
        {
            X = packet.X;
            Y = packet.Y;
            Command = packet.Command;
            IsAccelerator = packet.IsAccelerator;
            IsMnemonic = packet.IsMnemonic;
        }

        public override string ToString()
        {
            var accel = IsAccelerator ? " Accelerator " : string.Empty;
            var mnem = IsMnemonic ? " Mnemonic " : string.Empty;
            return FormattableString.Invariant($"{Command} ({X}, {Y}){accel}{mnem}");
        }
    }
}

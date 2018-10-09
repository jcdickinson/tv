/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Drawing;
using TerminalVelocity.Direct2D.DirectX;
using TerminalVelocity.Eventing;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class InitializeEvent : Event<DirectXRenderEventLoop, InitializeEventData>
    {
        public InitializeEvent(DirectXRenderEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct InitializeEventData
    {
        public IntPtr Hwnd;
        public SizeF Size;

        public InitializeEventData(IntPtr hwnd, SizeF size)
        {
            Hwnd = hwnd;
            Size = size;
        }

        public override string ToString() => FormattableString.Invariant($"{Hwnd} {Size}");
    }
}

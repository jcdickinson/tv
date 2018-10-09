/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface IFont : IDisposable
    {
        string Family { get; }
        float Size { get; }

        IText CreateText(IBrush defaultBrush, ReadOnlyMemory<char> text, SizeF layout);
    }
}

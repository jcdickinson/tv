/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface IText : IDisposable
    {
        IFont DefaultFont { get; }
        ReadOnlyMemory<char> Text { get; }
        SizeF Layout { get; }
        IBrush DefaultBrush { get; }

        void SetBrush(TextRange range, IBrush brush);
        void SetFont(TextRange range, IFont font);
        TextMetrics CalculateMetrics();

        void Draw(PointF location);
    }
}

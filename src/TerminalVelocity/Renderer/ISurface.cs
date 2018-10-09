/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Drawing;

namespace TerminalVelocity.Renderer
{
    public interface ISurface
    {
        IFont CreateFont(string family, float size);
        ISolidColorBrush CreateSolidColorBrush(in Color color);
        void FillRectangle(IBrush brush, in RectangleF rectangle);
    }
}

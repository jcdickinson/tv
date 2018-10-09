/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Drawing;

namespace TerminalVelocity.Preferences
{
    public class TerminalConfiguration
    {
        public Configurable<string> Font { get; }
        public Configurable<int> FontSize { get; }
        public Configurable<Color> Color0 { get; }
        public Configurable<Color> Color1 { get; }

        public TerminalConfiguration()
        {
            Font = "Fira Code";
            Color0 = Color.Black;
            Color1 = Color.White;
            FontSize = 16;
        }
    }
}

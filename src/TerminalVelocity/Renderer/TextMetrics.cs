/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

namespace TerminalVelocity.Renderer
{
    /// <summary>
    /// Contains the metrics associated with text after layout.
    /// </summary>
    public readonly struct TextMetrics
    {
        /// A value that indicates the left-most point of formatted text relative to the
        /// layout box, while excluding any glyph overhang.
        /// </summary>
        public float Left { get; }

        /// A value that indicates the top-most point of formatted text relative to the layout
        ///box, while excluding any glyph overhang.
        /// </summary>
        public float Top { get; }

        /// <summary>
        /// A value that indicates the width of the formatted text, while ignoring trailing
        /// whitespace at the end of each line.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// The width of the formatted text, taking into account the trailing whitespace
        /// at the end of each line.
        /// </summary>
        public float WidthIncludingTrailingWhitespace { get; }

        /// <summary>
        /// The height of the formatted text. The height of an empty string is set to the
        /// same value as that of the default font.
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// The initial width given to the layout. It can be either larger or smaller than
        /// the text content width, depending on whether the text was wrapped.
        /// </summary>
        public float LayoutWidth { get; }

        /// <summary>
        /// Initial height given to the layout. Depending on the length of the text, it may
        /// be larger or smaller than the text content height.
        /// </summary>
        public float LayoutHeight { get; }

        /// <summary>
        /// The maximum reordering count of any line of text, used to calculate the most
        /// number of hit-testing boxes needed. If the layout has no bidirectional text,
        /// or no text at all, the minimum level is 1.
        /// </summary>
        public int MaxBidiReorderingDepth { get; }

        /// <summary>
        /// Total number of lines.
        /// </summary>
        public int LineCount { get; }

        public TextMetrics(
            float left, 
            float top, 
            float width, 
            float widthIncludingTrailingWhitespace, 
            float height, float layoutWidth, 
            float layoutHeight, 
            int maxBidiReorderingDepth, 
            int lineCount)
        {
            Left = left;
            Top = top;
            Width = width;
            WidthIncludingTrailingWhitespace = widthIncludingTrailingWhitespace;
            Height = height;
            LayoutWidth = layoutWidth;
            LayoutHeight = layoutHeight;
            MaxBidiReorderingDepth = maxBidiReorderingDepth;
            LineCount = lineCount;
        }
    }
}

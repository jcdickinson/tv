/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;

namespace TerminalVelocity
{
    internal static class SpanExtensions
    {
        public static T? Optional<T>(this Span<T> span, int index)
            where T : struct
            => index >= span.Length ? new T?() : new T?(span[index]);

        public static T? Optional<T>(this ReadOnlySpan<T> span, int index)
            where T : struct
            => index >= span.Length ? new T?() : new T?(span[index]);
    }
}

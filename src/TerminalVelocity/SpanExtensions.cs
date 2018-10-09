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

using System;

namespace TerminalVelocity.Renderer
{
    public readonly struct TextRange
    {
        public int Offset { get; }
        public int Length { get; }

        public TextRange(int offset, int length)
        {
            if (offset <= 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Offset = offset;
            Length = length;
        }

        public static implicit operator TextRange((int Offset, int Length) range)
            => new TextRange(range.Offset, range.Length);
    }
}

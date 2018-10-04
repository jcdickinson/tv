using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TerminalVelocity.Emulator
{
    [StructLayout(LayoutKind.Explicit, Size = Length)]
    public struct CharSet
    {
        public const int Length = 256 * sizeof(char);
        public static readonly CharSet Ascii = CreateAscii();
        public static readonly CharSet SpecialCharacterAndLineDrawing = CreateSpecialCharacterAndLineDrawing();

        public char this[byte index]
        {
            get
            {
                var bytes = MemoryMarshal.Cast<CharSet, char>(MemoryMarshal.CreateSpan(ref this, 1));
                return bytes[index];
            }
            set
            {
                var bytes = MemoryMarshal.Cast<CharSet, char>(MemoryMarshal.CreateSpan(ref this, 1));
                bytes[index] = value;
            }
        }
        
        public char this[char index]
        {
            get
            {
                var i = (uint)index;
                if (i > 255) return index;
                return this[(byte)index];
            }
            set
            {
                var i = (uint)index;
                if (i > 255) throw new ArgumentOutOfRangeException(nameof(index));
                this[(byte)index] = value;
            }
        }

        private static CharSet CreateAscii()
        {
            var result = new CharSet();
            for (var i = 0U; i < 256; i++)
                result[(char)i] = (char)i;
            return result;
        }

        private static CharSet CreateSpecialCharacterAndLineDrawing()
        {
            var result = Ascii;
            result['`'] = '◆';
            result['a'] = '▒';
            result['b'] = '\t';
            result['c'] = '\x000c';
            result['d'] = '\r';
            result['e'] = '\n';
            result['f'] = '°';
            result['g'] = '±';
            result['h'] = '\x2424';
            result['i'] = '\x000b';
            result['j'] = '┘';
            result['k'] = '┐';
            result['l'] = '┌';
            result['m'] = '└';
            result['n'] = '┼';
            result['o'] = '⎺';
            result['p'] = '⎻';
            result['q'] = '─';
            result['r'] = '⎼';
            result['s'] = '⎽';
            result['t'] = '├';
            result['u'] = '┤';
            result['v'] = '┴';
            result['w'] = '┬';
            result['x'] = '│';
            result['y'] = '≤';
            result['z'] = '≥';
            result['{'] = 'π';
            result['|'] = '≠';
            result['}'] = '£';
            result['~'] = '·';
            return result;
        }
    }
}
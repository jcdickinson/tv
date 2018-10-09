/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Runtime.InteropServices;

namespace TerminalVelocity.Terminal
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
                Span<char> bytes = MemoryMarshal.Cast<CharSet, char>(MemoryMarshal.CreateSpan(ref this, 1));
                return bytes[index];
            }
            set
            {
                Span<char> bytes = MemoryMarshal.Cast<CharSet, char>(MemoryMarshal.CreateSpan(ref this, 1));
                bytes[index] = value;
            }
        }

        public char this[char index]
        {
            get => index > 255 ? index : this[(byte)index];
            set
            {
                uint i = index;
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
            CharSet result = Ascii;
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

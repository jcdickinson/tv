/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Text;

namespace TerminalVelocity.Terminal
{
    internal struct CharacterParser
    {
        public delegate bool CharacterEvent(ReadOnlySpan<byte> next, out ReadOnlySpan<char> result);

        private readonly char[] _characters;
        private readonly UTF8Encoding _utf8;
        private readonly Decoder _decoder;

        public CharacterParser(UTF8Encoding utf8, int maxCharacters = 4096)
        {
            _characters = new char[maxCharacters];
            _utf8 = utf8;
            _decoder = utf8.GetDecoder();
        }

        public static CharacterParser Create(int maxCharacters = 4096)
            => new CharacterParser(new UTF8Encoding(false, false), maxCharacters);

        public bool TryParseAscii(ReadOnlySpan<byte> next, out ReadOnlySpan<char> result)
        {
            for (var i = 0; i < next.Length; i++)
                _characters[i] = (char)next[i];
            result = _characters.AsSpan(0, 1);
            return true;
        }

        public bool TryParseUtf8(ReadOnlySpan<byte> next, out ReadOnlySpan<char> result)
        {
            var length = _decoder.GetChars(next, _characters, false);
            if (length != 0)
            {
                _decoder.Reset();
                result = _characters.AsSpan(0, length);
                return true;
            }
            result = default;
            return false;
        }
    }
}

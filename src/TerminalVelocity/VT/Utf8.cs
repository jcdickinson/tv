using System;
using System.Text;

namespace TerminalVelocity.VT
{
    internal struct Utf8
    {
        // https://bjoern.hoehrmann.de/utf-8/decoder/dfa/
        
        private const char Unknown = '\uFFFD';
        private readonly Decoder _utf8;
        private readonly byte[] _buffer;
        private readonly char[] _result;

        public Utf8(bool sentinel)
        {
            var encoding = new UTF8Encoding(false, false);
            _utf8 = encoding.GetDecoder();
            _buffer = new byte[1];
            _result = new char[encoding.GetMaxCharCount(6)];
        }

        public ReadOnlySpan<char> Provide(byte next)
        {
            _result[0] = (char)next;
            return _result.AsSpan(0, 1);
        }

        public bool Process(byte next, out ReadOnlySpan<char> result)
        {
            _buffer[0] = next;
            var length = _utf8.GetChars(_buffer, _result, false);
            if (length != 0)
            {
                _utf8.Reset();
                result = _result.AsSpan(0, length);
                return true;
            }
            result = default;
            return false;
        }
    }
}
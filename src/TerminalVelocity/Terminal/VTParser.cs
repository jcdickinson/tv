/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

/* The VT parser is derived from code published by Joe Wilm:
 * https://github.com/jwilm/vte
 *
 * Copyright (c) 2016 Joe Wilm
 * 
 * Permission is hereby granted, free of charge, to any
 * person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the
 * Software without restriction, including without
 * limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice
 * shall be included in all copies or substantial portions
 * of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF
 * ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT
 * SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
 * IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerminalVelocity.Terminal
{
    internal partial struct VTParser
    {
        [Flags]
        private enum ParseOptions : byte
        {
            None = 0,
            CollectingParameters = 0b0000_0100,
            IgnoringIntermediates = 0b0000_00001,
            IgnoringParameters = 0b0000_00010,
            Ignoring = IgnoringIntermediates | IgnoringParameters
        }

        private ParserState _state;
        private readonly byte[] _intermediates;
        private readonly long[] _params;
        private readonly ReadOnlyMemory<byte>[] _oscParams;
        private readonly byte[] _oscRaw;

        private ParseOptions _options;
        private int _intermediateIndex;
        private long _param;
        private int _numParams;
        private int _oscIndex;
        private int _previousOscEnd;
        private int _oscNumParams;

        private readonly VTParserEvents _events;
        private readonly CharacterParser.CharacterEvent _utf8;
        private readonly CharacterParser.CharacterEvent _ascii;

        public VTParser(
            int maxIntermediates = 2,
            int maxParameters = 16,
            int maxOsCommandRaw = 1024,
            VTParserEvents events = default,
            CharacterParser.CharacterEvent utf8 = default,
            CharacterParser.CharacterEvent ascii = default)
        {

            _state = ParserState.Ground;
            _intermediates = new byte[maxIntermediates];
            _params = new long[maxParameters];
            _oscRaw = new byte[maxOsCommandRaw];
            _oscParams = new ReadOnlyMemory<byte>[maxParameters];

            _options = default;
            _intermediateIndex = default;
            _param = default;
            _numParams = default;
            _oscIndex = default;
            _previousOscEnd = default;
            _oscNumParams = default;

            _events = events;
            _utf8 = utf8;
            _ascii = ascii;
        }

        public void Process(ReadOnlySpan<byte> next)
        {
            for (var i = 0; i < next.Length; i++)
                Process(next[i]);
        }

        private void Process(byte next)
        {
            if (_state == ParserState.Utf8)
            {
                ProcessUtf8(next);
                return;
            }

            // Handle state changes in the anywhere state before evaluating changes
            // for current state.
            ParserStateAction change = ParserState.Anywhere.GetStateChange(next);
            if (change.IsEmpty)
                change = _state.GetStateChange(next);

            PerformChange(change, next);
        }

        private void ProcessUtf8(byte next)
        {
            ReadOnlySpan<byte> span = MemoryMarshal.CreateReadOnlySpan(ref next, 1);
            if (!_utf8(span, out ReadOnlySpan<char> result))
                return;

            _events.Print?.Invoke(result);
            _state = ParserState.Ground;
        }

        private void PerformChange(ParserStateAction change, byte next)
        {
            if (change.State == ParserState.Anywhere)
            {
                PerformAction(change, next);
                return;
            }

            PerformAction(new ParserStateAction(_state).WithExitAction(), 0);
            PerformAction(change, next);
            PerformAction(change.WithEntryAction(), 0);
            if (change.State != ParserState.Anywhere)
                _state = change.State;
        }

        private void PerformAction(ParserStateAction change, byte next)
        {
            if (change.Action == ParserAction.None)
                return;

            (ParserState _, ParserAction action) = change;

            switch (action)
            {
                case ParserAction.Print:
                    ReadOnlySpan<byte> span = MemoryMarshal.CreateReadOnlySpan(ref next, 1);
                    if (_ascii(span, out ReadOnlySpan<char> characters))
                        _events.Print?.Invoke(characters);
                    return;
                case ParserAction.Execute:
                    _events.Execute?.Invoke((ControlCode)next);
                    return;
                case ParserAction.Hook:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;

                    _events.Hook?.Invoke(
                        _intermediates.AsSpan(0, _intermediateIndex),
                        _params.AsSpan(0, _numParams),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring));

                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case ParserAction.Put:
                    _events.Put?.Invoke(next);
                    return;
                case ParserAction.OscStart:
                    _oscIndex = 0;
                    _oscNumParams = 0;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case ParserAction.OscPut:
                    if (_oscIndex == _oscRaw.Length)
                        return;

                    // Param separator
                    if (next == ';')
                    {
                        if (_oscNumParams == _oscParams.Length)
                        {
                            _options |= ParseOptions.IgnoringParameters;
                            return;
                        }
                        // First param is special - 0 to current byte index
                        else if (_oscNumParams == 0)
                        {
                            _oscParams[_oscNumParams++] = _oscRaw.AsMemory(0, _oscIndex);
                            _previousOscEnd = _oscIndex;
                            return;
                        }
                        // All other params depend on previous indexing
                        else
                        {
                            _oscParams[_oscNumParams++] = _oscRaw.AsMemory(_previousOscEnd, _oscIndex - _previousOscEnd);
                            _previousOscEnd = _oscIndex;
                            return;
                        }
                    }

                    _oscRaw[_oscIndex++] = next;
                    return;
                case ParserAction.OscEnd:
                    if (_oscIndex == _oscRaw.Length)
                        return;

                    if (_oscNumParams == _oscParams.Length)
                        _options |= ParseOptions.IgnoringParameters;
                    else
                        _oscParams[_oscNumParams++] = _oscNumParams == 0
                            ? (ReadOnlyMemory<byte>)_oscRaw.AsMemory(0, _oscIndex)
                            : (ReadOnlyMemory<byte>)_oscRaw.AsMemory(_previousOscEnd, _oscIndex - _previousOscEnd);

                    ReadOnlySpan<ReadOnlyMemory<byte>> param = _oscParams.AsSpan(0, _oscNumParams);
                    OsCommand command = OsCommand.Unknown;
                    if (param.Length > 0 && TryParseInt16(param[0].Span, out short commandIndex))
                    {
                        command = (OsCommand)commandIndex;
                        param = param.Slice(1);
                    }

                    _events.OsCommand?.Invoke(
                        command,
                        param,
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring));
                    return;
                case ParserAction.Unhook:
                    _events.Unhook?.Invoke();
                    return;
                case ParserAction.CsiDispatch:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;

                    _events.ControlSequence?.Invoke(
                        (ControlSequenceCommand)next,
                        _intermediates.AsSpan(0, _intermediateIndex),
                        _params.AsSpan(0, _numParams),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring));

                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case ParserAction.EscDispatch:
                    _events.EscapeSequence?.Invoke(
                        (EscapeCommand)next,
                        _intermediates.AsSpan(0, _intermediateIndex),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring));
                    return;
                case ParserAction.Ignore:
                case ParserAction.None:
                    return;
                case ParserAction.Collect:
                    if (_intermediateIndex == _intermediates.Length)
                        _options |= ParseOptions.IgnoringIntermediates;
                    else
                        _intermediates[_intermediateIndex++] = next;
                    return;
                case ParserAction.Param:
                    if (next == ';')
                    {
                        if (_numParams == _params.Length - 1)
                        {
                            _options |= ParseOptions.IgnoringParameters;
                            return;
                        }
                        _params[_numParams++] = _param;
                        _param = 0;
                        _options &= ~ParseOptions.CollectingParameters;
                        return;
                    }
                    else if (!_options.HasFlag(ParseOptions.IgnoringParameters))
                    {
                        MulAdd(ref _param, next - '0');
                        _options |= ParseOptions.CollectingParameters;
                    }
                    return;
                case ParserAction.Clear:
                    _intermediateIndex = 0;
                    _numParams = 0;
                    _options &= ~ParseOptions.IgnoringIntermediates;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case ParserAction.BeginUtf8:
                    ProcessUtf8(next);
                    return;
            }
        }

        private static void MulAdd(ref long a, int b)
        {
            var na = (a * 10) + b;
            a = a < 0 && na >= 0
                ? long.MinValue
                : a >= 0 && na < 0
                    ? long.MaxValue
                    : na;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseInt16(ReadOnlySpan<byte> raw, out short result)
        {
            result = 0;
            if (raw.Length == 0) return false;

            for (var i = 0; i < raw.Length; i++)
            {
                result = (short)((result * 10) + (raw[i] - (byte)'0'));
                if (result < 0)
                {
                    result = 0;
                    return false;
                }
            }
            return true;
        }
    }
}

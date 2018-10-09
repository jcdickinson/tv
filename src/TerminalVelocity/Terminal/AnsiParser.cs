/* The ANSI parser is derived from code published by Joe Wilm and Alacritty Contributors:
 * https://github.com/jwilm/alacritty
 *
 * Copyright 2016 Joe Wilm, The Alacritty Project Contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TerminalVelocity.Terminal
{
    internal struct AnsiParser
    {

        private const int G0 = 0;
        private const int G1 = 1;
        private const int G2 = 2;
        private const int G3 = 3;

        private static readonly byte[] _cursorShape = Encoding.ASCII.GetBytes("CursorShape=");

        private readonly char[] _charBuffer;
        private readonly byte[] _byteBuffer;
        private readonly CharSet[] _activeCharSets;
        private int _activeCharSetIndex;
        private char? _precedingChar;

        private readonly AnsiParserEvents _events;
        private readonly CharacterParser.CharacterEvent _utf8;

        public AnsiParser(
            AnsiParserEvents events = default,
            CharacterParser.CharacterEvent utf8 = default)
        {
            _activeCharSets = new[]
            {
                CharSet.Ascii,
                CharSet.Ascii,
                CharSet.Ascii,
                CharSet.Ascii
            };

            _activeCharSetIndex = 0;
            _charBuffer = new char[4096];
            _byteBuffer = new byte[4096];

            _events = events;
            _utf8 = utf8;
            _precedingChar = default;
        }

        public void OnPrint(ReadOnlySpan<char> characters)
        {
            if (characters.Length > 0)
            {
                _precedingChar = characters[characters.Length - 1];
                _events.Input?.Invoke(MapCharacters(characters));
            }
        }

        public void OnExecute(ControlCode controlCode)
        {
            switch (controlCode)
            {
                case ControlCode.HorizontalTabulation:
                    _events.PutTab?.Invoke(1);
                    break;
                case ControlCode.Backspace:
                    _events.Backspace?.Invoke();
                    break;
                case ControlCode.CarriageReturn:
                    _events.CarriageReturn?.Invoke();
                    break;
                case ControlCode.FormFeed:
                case ControlCode.VerticalTabulation:
                case ControlCode.LineFeed:
                    _events.LineFeed?.Invoke();
                    break;
                case ControlCode.Bell:
                    _events.Bell?.Invoke();
                    break;
                case ControlCode.ShiftIn:
                    _activeCharSetIndex = G0;
                    break;
                case ControlCode.ShiftOut:
                    _activeCharSetIndex = G1;
                    break;
                case ControlCode.NewLine:
                    _events.NewLine?.Invoke();
                    break;
                case ControlCode.HorizontalTabulationSet:
                    _events.SetHorizontalTabStop?.Invoke();
                    break;
                case ControlCode.SingleCharacterIntroducer:
                    _events.IdentifyTerminal?.Invoke();
                    break;
            }
        }

        public void OnOsCommand(OsCommand command, ReadOnlySpan<ReadOnlyMemory<byte>> parameters, IgnoredData ignored = default)
        {
            Color color;
            ReadOnlySpan<char> characters;
            switch (command)
            {
                case OsCommand.SetWindowIconAndTitle:
                case OsCommand.SetWindowTitle:
                    // Set window title.
                    if (parameters.Length > 0 &&
                        _utf8(parameters[0].Span, out characters))
                        _events.SetTitle?.Invoke(characters);
                    break;
                case OsCommand.SetWindowIcon: break;
                case OsCommand.SetColor:
                    // Set color index.
                    if (parameters.Length % 2 != 0)
                        break;
                    for (var i = 0; i < parameters.Length; i += 2)
                    {
                        if (TryParseByte(parameters[i].Span, out var index) &&
                            TryParseColor(parameters[i + 1].Span, out color))
                            _events.SetColor?.Invoke((NamedColor)index, color);
                    }
                    break;
                case OsCommand.SetForegroundColor:
                    if (parameters.Length > 0 &&
                        TryParseColor(parameters[0].Span, out color))
                        _events.SetColor?.Invoke(NamedColor.Foreground, color);
                    break;
                case OsCommand.SetBackgroundColor:
                    if (parameters.Length > 0 &&
                        TryParseColor(parameters[0].Span, out color))
                        _events.SetColor?.Invoke(NamedColor.Background, color);
                    break;
                case OsCommand.SetCursorColor:
                    if (parameters.Length > 0 &&
                        TryParseColor(parameters[0].Span, out color))
                        _events.SetColor?.Invoke(NamedColor.Cursor, color);
                    break;
                case OsCommand.SetCursorStyle:
                    if (parameters.Length > 0 && parameters[0].Length > _cursorShape.Length &&
                        parameters[0].Span.Slice(0, _cursorShape.Length).SequenceEqual(_cursorShape))
                    {
                        switch ((CursorStyle)(parameters[0].Span[12] - (byte)'0'))
                        {
                            case CursorStyle.Beam:
                                _events.SetCursor?.Invoke(CursorStyle.Beam);
                                break;
                            case CursorStyle.Block:
                                _events.SetCursor?.Invoke(CursorStyle.Block);
                                break;
                            case CursorStyle.Underline:
                                _events.SetCursor?.Invoke(CursorStyle.Underline);
                                break;
                        }
                    }
                    break;
                case OsCommand.SetClipboard:
                    if (parameters.Length > 1 &&
                        (parameters[1].Length == 0 || parameters[1].Span[0] != '?') &&
                        TryParseBase64(parameters[1].Span, out ReadOnlySpan<byte> base64) &&
                        _utf8(base64, out characters))
                        _events.SetClipboard?.Invoke(characters);
                    break;
                case OsCommand.ResetColor:
                    if (parameters.Length == 0)
                    {
                        for (var i = 0; i < 257; i++)
                            _events.ResetColor?.Invoke((NamedColor)i);
                        break;
                    }

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (TryParseByte(parameters[i].Span, out var index))
                            _events.ResetColor?.Invoke((NamedColor)index);
                    }
                    break;
                case OsCommand.ResetForegroundColor:
                    _events.ResetColor?.Invoke(NamedColor.Foreground);
                    break;
                case OsCommand.ResetBackgroundColor:
                    _events.ResetColor?.Invoke(NamedColor.Background);
                    break;
                case OsCommand.ResetCursorColor:
                    _events.ResetColor?.Invoke(NamedColor.Cursor);
                    break;
            }
        }

        public void OnEscapeSequenceEvent(EscapeCommand command, ReadOnlySpan<byte> intermediates, IgnoredData ignored = default)
        {
            int index;
            switch (command)
            {
                case EscapeCommand.ConfigureAsciiCharSet:
                    if (intermediates.Length > 0 &&
                        TryParseCharSetIndex(intermediates[0], out index))
                        _activeCharSets[index] = CharSet.Ascii;
                    break;
                case EscapeCommand.ConfigureSpecialCharSet:
                    if (intermediates.Length > 0 &&
                        TryParseCharSetIndex(intermediates[0], out index))
                        _activeCharSets[index] = CharSet.SpecialCharacterAndLineDrawing;
                    break;
                case EscapeCommand.LineFeed:
                    _events.LineFeed?.Invoke();
                    break;
                case EscapeCommand.NextLine:
                    _events.LineFeed?.Invoke();
                    _events.CarriageReturn?.Invoke();
                    break;
                case EscapeCommand.ReverseIndex:
                    _events.ReverseIndex?.Invoke();
                    break;
                case EscapeCommand.IdentifyTerminal:
                    _events.IdentifyTerminal?.Invoke();
                    break;
                case EscapeCommand.ResetState:
                    _events.ResetState?.Invoke();
                    break;
                case EscapeCommand.SaveCursorPosition:
                    _events.SaveCursorPosition?.Invoke();
                    break;
                case EscapeCommand.DecTest when (intermediates.Length != 0 && intermediates[0] == '#'):
                    _events.DecTest?.Invoke();
                    break;
                case EscapeCommand.RestoreCursorPosition when (intermediates.Length == 0 || intermediates[0] != '#'):
                    _events.RestoreCursorPosition?.Invoke();
                    break;
                case EscapeCommand.SetKeypadApplicationMode:
                    _events.SetKeypadApplicationMode?.Invoke();
                    break;
                case EscapeCommand.UnsetKeypadApplicationMode:
                    _events.UnsetKeypadApplicationMode?.Invoke();
                    break;
            }
        }

        public void OnControlSequenceCommand(ControlSequenceCommand command, ReadOnlySpan<byte> intermediates, ReadOnlySpan<long> parameters, IgnoredData ignored = default)
        {
            var priv = intermediates.Optional(0).GetValueOrDefault(0) == '?';

            long? row, column;
            switch (command)
            {
                case ControlSequenceCommand.InsertBlank:
                    _events.InsertBlank?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.MoveUp:
                    _events.MoveUp?.Invoke(parameters.Optional(0).GetValueOrDefault(1), false);
                    break;
                case ControlSequenceCommand.RepeatPrecedingCharacter:
                    if (_precedingChar.HasValue)
                    {
                        var count = parameters.Optional(0).GetValueOrDefault(1);
                        var c = _precedingChar.Value;
                        ReadOnlySpan<char> chr = MapCharacters(MemoryMarshal.CreateReadOnlySpan(ref c, 1));
                        for (var i = 0; i < count; i++)
                            _events.Input?.Invoke(chr);
                    }
                    break;
                case ControlSequenceCommand.MoveDown1:
                case ControlSequenceCommand.MoveDown2:
                    _events.MoveDown?.Invoke(parameters.Optional(0).GetValueOrDefault(1), false);
                    break;
                case ControlSequenceCommand.IdentifyTerminal:
                    _events.IdentifyTerminal?.Invoke();
                    break;
                case ControlSequenceCommand.MoveForward1:
                case ControlSequenceCommand.MoveForward2:
                    _events.MoveForward?.Invoke(parameters.Optional(0).GetValueOrDefault(1), false);
                    break;
                case ControlSequenceCommand.MoveBackward:
                    _events.MoveBackward?.Invoke(parameters.Optional(0).GetValueOrDefault(1), false);
                    break;
                case ControlSequenceCommand.MoveDownAndCr:
                    _events.MoveDown?.Invoke(parameters.Optional(0).GetValueOrDefault(1), true);
                    break;
                case ControlSequenceCommand.MoveUpAndCr:
                    _events.MoveUp?.Invoke(parameters.Optional(0).GetValueOrDefault(1), true);
                    break;
                case ControlSequenceCommand.ClearTabulation:
                    var tabClearMode = (TabulationClearMode)parameters.Optional(0).GetValueOrDefault(0);
                    _events.ClearTabulation?.Invoke(tabClearMode);
                    break;
                case ControlSequenceCommand.GotoColumn1:
                case ControlSequenceCommand.GotoColumn2:
                    column = parameters.Optional(0).GetValueOrDefault(1) - 1;
                    _events.Goto?.Invoke(column: column);
                    break;
                case ControlSequenceCommand.Goto1:
                case ControlSequenceCommand.Goto2:
                    column = parameters.Optional(0).GetValueOrDefault(1) - 1;
                    row = parameters.Optional(1).GetValueOrDefault(1) - 1;
                    _events.Goto?.Invoke(column, row);
                    break;
                case ControlSequenceCommand.MoveForwardTabs:
                    _events.MoveForward?.Invoke(parameters.Optional(0).GetValueOrDefault(1), true);
                    break;
                case ControlSequenceCommand.ClearScreen:
                    var clearScreenMode = (ClearMode)parameters.Optional(0).GetValueOrDefault(0);
                    _events.ClearScreen?.Invoke(clearScreenMode);
                    break;
                case ControlSequenceCommand.ClearLine:
                    var clearLineMode = (LineClearMode)parameters.Optional(0).GetValueOrDefault(0);
                    _events.ClearLine?.Invoke(clearLineMode);
                    break;
                case ControlSequenceCommand.ScrollUp:
                    _events.ScrollUp?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.ScrollDown:
                    _events.ScrollDown?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.InsertBlankLines:
                    _events.InsertBlankLines?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.UnsetMode:
                    for (var i = 0; i < parameters.Length; i++)
                        _events.UnsetMode?.Invoke((Mode)parameters[i]);
                    break;
                case ControlSequenceCommand.DeleteLines:
                    _events.DeleteLines?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.EraseChars:
                    _events.EraseCharacters?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.DeleteChars:
                    _events.DeleteCharacters?.Invoke(parameters.Optional(0).GetValueOrDefault(1));
                    break;
                case ControlSequenceCommand.MoveBackwardTabs:
                    _events.MoveBackward?.Invoke(parameters.Optional(0).GetValueOrDefault(1), true);
                    break;
                case ControlSequenceCommand.GotoLine:
                    _events.Goto?.Invoke(line: parameters.Optional(0).GetValueOrDefault(1) - 1);
                    break;
                case ControlSequenceCommand.SetMode:
                    for (var i = 0; i < parameters.Length; i++)
                        _events.SetMode?.Invoke((Mode)parameters[i]);
                    break;
                case ControlSequenceCommand.TerminalAttribute:
                    if (parameters.Length == 0)
                    {
                        _events.TerminalAttribute?.Invoke(
                            TerminalAttribute.Reset);
                        break;
                    }

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var index = parameters[i];

                        if (index < 30)
                        {
                            _events.TerminalAttribute?.Invoke(
                                (TerminalAttribute)index);
                            continue;
                        }

                        if (index == 38 || index == 48)
                        {
                            var consumed = TryParseColor(parameters.Slice(i), out NamedColor? ix, out Color? color);
                            if (consumed == 0) break;
                            _events.TerminalAttribute?.Invoke(
                                (TerminalAttribute)index, index: ix, color: color);
                            i += consumed;
                            continue;
                        }

                        if (index == 39)
                        {
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetForeground, index: NamedColor.Foreground);
                            continue;
                        }

                        if (index < 40)
                        {
                            var ix = (NamedColor)(index - 30);
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetForeground, index: ix);
                            continue;
                        }

                        if (index == 49)
                        {
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetBackground, index: NamedColor.Background);
                            continue;
                        }

                        if (index < 50)
                        {
                            var ix = (NamedColor)(index - 40);
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetForeground, index: ix);
                            continue;
                        }

                        if (index < 90)
                            continue;

                        if (index < 100)
                        {
                            var ix = (NamedColor)(index - 90 + (int)NamedColor.BrightBlack);
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetForeground, index: ix);
                            continue;
                        }

                        if (index < 108)
                        {
                            var ix = (NamedColor)(index - 100 + (int)NamedColor.BrightBlack);
                            _events.TerminalAttribute?.Invoke(
                                TerminalAttribute.SetBackground, index: ix);
                            continue;
                        }
                    }
                    break;
                case ControlSequenceCommand.DeviceStatus:
                    _events.DeviceStatus?.Invoke(parameters.Optional(0).GetValueOrDefault(0));
                    break;
                case ControlSequenceCommand.SetScrollingRegion:
                    if (priv) break;
                    _events.SetScrollingRegion?.Invoke(
                        parameters.Optional(0) - 1,
                        parameters.Optional(1) - 1);
                    break;
                case ControlSequenceCommand.SaveCursorPosition:
                    _events.SaveCursorPosition?.Invoke();
                    break;
                case ControlSequenceCommand.RestoreCursorPosition:
                    _events.RestoreCursorPosition?.Invoke();
                    break;
                case ControlSequenceCommand.SetCursorStyle:
                    switch (parameters.Optional(0).GetValueOrDefault(0))
                    {
                        case 0: _events.SetCursorStyle?.Invoke(default); break;
                        case 1: case 2: _events.SetCursorStyle?.Invoke(CursorStyle.Block); break;
                        case 3: case 4: _events.SetCursorStyle?.Invoke(CursorStyle.Underline); break;
                        case 5: case 6: _events.SetCursorStyle?.Invoke(CursorStyle.Beam); break;
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<char> MapCharacters(ReadOnlySpan<char> span)
        {
            ref CharSet chars = ref _activeCharSets[_activeCharSetIndex];
            for (var i = 0; i < span.Length; i++)
                _charBuffer[i] = chars[span[i]];
            return _charBuffer.AsSpan(0, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseByte(ReadOnlySpan<byte> raw, out byte result)
        {
            result = 0;
            if (raw.Length == 0) return false;

            for (var i = 0; i < raw.Length; i++)
                result = (byte)((result * 10) + (raw[i] - (byte)'0'));
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseColor(ReadOnlySpan<byte> raw, out Color color)
        {
            color = default;
            // Expect that color argument looks like "rgb:xx/xx/xx" or "#xxxxxx"
            if (raw.Length < 7)
                return false;

            var i = 0;
            var mode = raw[i++];
            byte r, g, b;
            if (mode == 'r')
            {
                if (raw.Length < 12) return false;
                if (raw[i++] != 'g') return false;
                if (raw[i++] != 'b') return false;
                if (raw[i++] != ':') return false;

                r = Hex(raw[i++], raw[i++]);
                if (raw[i++] != '/') return false;
                g = Hex(raw[i++], raw[i++]);
                if (raw[i++] != '/') return false;
                b = Hex(raw[i++], raw[i++]);
            }
            else if (mode == '#')
            {
                r = Hex(raw[i++], raw[i++]);
                g = Hex(raw[i++], raw[i++]);
                b = Hex(raw[i++], raw[i++]);
            }
            else
            {
                return false;
            }

            color = Color.FromArgb(r, g, b);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Hex(byte b1, byte b2)
        {
            b1 = Hexit(b1);
            b2 = Hexit(b2);
            b1 <<= 4;
            b1 |= b2;
            return b1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Hexit(byte value)
        {
            // Ignore errors in favor of speed.
            // https://gist.github.com/jcdickinson/9a4205287ae107e9f4e5f6764f432db9

            var h = (byte)(value >> 6);
            h |= (byte)(h << 3);
            h += (byte)(value & 0b1111);
            return h;
        }

        private bool TryParseBase64(ReadOnlySpan<byte> raw, out ReadOnlySpan<byte> result)
        {
            if (Base64.DecodeFromUtf8(raw, _byteBuffer, out var consumed, out var bytesWritten, true) == OperationStatus.Done)
            {
                result = _byteBuffer.AsSpan(0, bytesWritten);
                return true;
            }
            result = default;
            return false;
        }

        private static bool TryParseCharSetIndex(byte raw, out int result)
        {
            switch (raw)
            {
                case (byte)'(': result = G0; return true;
                case (byte)')': result = G1; return true;
                case (byte)'*': result = G2; return true;
                case (byte)'+': result = G3; return true;
            }

            result = -1;
            return false;
        }

        private static int TryParseColor(ReadOnlySpan<long> args, out NamedColor? index, out Color? color)
        {
            if (args.Length <= 1)
            {
                index = default;
                color = default;
                return 0;
            }

            switch (args[1])
            {
                case 2:  // RGB
                    if (args.Length <= 4)
                    {
                        index = default;
                        color = default;
                        return 0;
                    }

                    var r = args[2];
                    var g = args[3];
                    var b = args[4];

                    if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
                    {
                        index = default;
                        color = default;
                        return 4;
                    }

                    index = default;
                    color = Color.FromArgb((int)r, (int)g, (int)b);
                    return 4;
                case 5: // index
                    if (args.Length <= 2)
                    {
                        index = default;
                        color = default;
                        return 0;
                    }

                    index = (NamedColor)args[2];
                    color = default;
                    return 2;
                default:
                    index = default;
                    color = default;
                    return 0;
            }
        }
    }
}

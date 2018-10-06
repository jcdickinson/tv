using System;
using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerminalVelocity.Emulator.Events;
using TerminalVelocity.Eventing;
using TerminalVelocity.VT;

namespace TerminalVelocity.Emulator
{
    // https://github.com/jwilm/alacritty/blob/master/src/term/mod.rs
    // https://github.com/jwilm/alacritty/blob/master/src/ansi.rs

    public sealed class TerminalEmulator
    {

        private const int G0 = 0;
        private const int G1 = 1;
        private const int G2 = 2;
        private const int G3 = 3;

        private static readonly char[] _horizontalTabulator = "\t".ToCharArray();
        private static readonly char[] _cr = "\r".ToCharArray();
        private static readonly char[] _lf = "\n".ToCharArray();
        private static readonly char[] _crLf = "\r\n".ToCharArray();
        private static readonly byte[] _cursorShape = Encoding.ASCII.GetBytes("CursorShape=");

        private readonly char[] _charBuffer;
        private readonly byte[] _byteBuffer;
        private readonly CharSet[] _activeCharSets;
        private readonly UTF8Encoding _utf8;
        private int _activeCharSetIndex;

        private readonly PrintEvent _printEvent;
        private readonly WhitespaceEvent _whitespaceEvent;
        private readonly DeleteEvent _deleteEvent;
        private readonly BellEvent _bellEvent;
        private readonly SetTabstopEvent _setTabstopEvent;
        private readonly IdentifyTerminalEvent _identifyTerminalEvent;
        private readonly SetWindowTitleEvent _setWindowTitleEvent;
        private readonly SetColorEvent _setColorEvent;
        private readonly SetCursorEvent _setCursorEvent;
        private readonly SetClipboardEvent _setClipboardEvent;
        private readonly ResetColorEvent _resetColorEvent;
        private readonly MoveCursorEvent _moveCursorEvent;
        private readonly StateEvent _stateEvent;

        public TerminalEmulator(
            VT.Events.PrintEvent onPrintEvent = null,
            VT.Events.ExecuteEvent onExecuteEvent = null,
            VT.Events.OsCommandEvent onOsCommandEvent = null,
            VT.Events.EscapeSequenceEvent onEscapeSequenceEvent = null,

            PrintEvent printEvent = null,
            WhitespaceEvent whitespaceEvent = null,
            DeleteEvent deleteEvent = null,
            BellEvent bellEvent = null,
            SetTabstopEvent setTabstopEvent = null,
            IdentifyTerminalEvent identifyTerminalEvent = null,
            SetWindowTitleEvent setWindowTitleEvent = null,
            SetColorEvent setColorEvent = null,
            SetCursorEvent setCursorEvent = null,
            SetClipboardEvent setClipboardEvent = null,
            ResetColorEvent resetColorEvent = null,
            MoveCursorEvent moveCursorEvent = null,
            StateEvent stateEvent = null)
        {
            _printEvent = printEvent;
            _whitespaceEvent = whitespaceEvent;
            _deleteEvent = deleteEvent;
            _bellEvent = bellEvent;
            _setTabstopEvent = setTabstopEvent;
            _identifyTerminalEvent = identifyTerminalEvent;
            _setWindowTitleEvent = setWindowTitleEvent;
            _setColorEvent = setColorEvent;
            _setCursorEvent = setCursorEvent;
            _setClipboardEvent = setClipboardEvent;
            _resetColorEvent = resetColorEvent;
            _moveCursorEvent = moveCursorEvent;
            _stateEvent = stateEvent;

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
            _utf8 = new UTF8Encoding(false, false);

            onPrintEvent?.Subscribe(OnPrintEvent);
            onExecuteEvent?.Subscribe(OnExecuteEvent);
            onOsCommandEvent?.Subscribe(OnOsCommandEvent);
            onEscapeSequenceEvent?.Subscribe(OnEscapeSequenceEvent);
        }

        private EventStatus OnPrintEvent(in VT.Events.PrintEventData print)
        {
            _printEvent?.Publish(new PrintEventData(MapCharacters(print.Characters.Span)));
            return EventStatus.Continue;
        }

        private EventStatus OnExecuteEvent(in VT.Events.ExecuteEventData execute)
        {
            switch (execute.ControlCode)
            {
                case ControlCode.HorizontalTabulation:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_horizontalTabulator, 1));
                    break;
                case ControlCode.Backspace:
                    _deleteEvent?.Publish(new DeleteEventData(Emulator.DeleteDirection.Backwards));
                    break;
                case ControlCode.CarriageReturn:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_cr, 1));
                    break;
                case ControlCode.FormFeed:
                case ControlCode.VerticalTabulation:
                case ControlCode.LineFeed:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_lf, 1));
                    break;
                case ControlCode.Bell:
                    _bellEvent?.Publish(new BellEventData());
                    break;
                case ControlCode.ShiftIn:
                    _activeCharSetIndex = G0;
                    break;
                case ControlCode.ShiftOut:
                    _activeCharSetIndex = G1;
                    break;
                case ControlCode.NextLine:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_crLf, 1));
                    break;
                case ControlCode.HorizontalTabulationSet:
                    _setTabstopEvent?.Publish(new SetTabstopEventData());
                    break;
                case ControlCode.SingleCharacterIntroducer:
                    _identifyTerminalEvent?.Publish(new IdentifyTerminalEventData());
                    break;
            }
            return EventStatus.Continue;
        }

        private EventStatus OnOsCommandEvent(in VT.Events.OsCommandEventData osc)
        {
            Color color;
            switch (osc.Command)
            {
                case OsCommand.SetWindowIconAndTitle:
                case OsCommand.SetWindowTitle:
                    // Set window title.
                    if (osc.Length > 0 &&
                        TryParseUtf8(osc[0], out ReadOnlyMemory<char> characters))
                        _setWindowTitleEvent?.Publish(new SetWindowTitleEventData(characters));
                    break;
                case OsCommand.SetWindowIcon: break;
                case OsCommand.SetColor:
                    // Set color index.
                    if (osc.Length % 2 != 0)
                        break;
                    for (var i = 0; i < osc.Length; i += 2)
                    {
                        if (TryParseByte(osc[i], out var index) &&
                            TryParseColor(osc[i + 1], out color))
                            _setColorEvent?.Publish(new SetColorEventData((NamedColor)index, color));
                    }
                    break;
                case OsCommand.SetForegroundColor:
                    if (osc.Length > 0 &&
                        TryParseColor(osc[0], out color))
                        _setColorEvent?.Publish(new SetColorEventData(NamedColor.Foreground, color));
                    break;
                case OsCommand.SetBackgroundColor:
                    if (osc.Length > 0 &&
                        TryParseColor(osc[0], out color))
                        _setColorEvent?.Publish(new SetColorEventData(NamedColor.Background, color));
                    break;
                case OsCommand.SetCursorColor:
                    if (osc.Length > 0 &&
                        TryParseColor(osc[0], out color))
                        _setColorEvent?.Publish(new SetColorEventData(NamedColor.Cursor, color));
                    break;
                case OsCommand.SetCursorStyle:
                    if (osc.Length > 0 && osc[0].Length > 12 &&
                        osc[0].Slice(0, _cursorShape.Length).SequenceEqual(_cursorShape))
                    {
                        switch ((CursorStyle)(osc[0][12] - (byte)'0'))
                        {
                            case CursorStyle.Beam:
                                _setCursorEvent?.Publish(new SetCursorEventData(CursorStyle.Beam));
                                break;
                            case CursorStyle.Block:
                                _setCursorEvent?.Publish(new SetCursorEventData(CursorStyle.Block));
                                break;
                            case CursorStyle.Underline:
                                _setCursorEvent?.Publish(new SetCursorEventData(CursorStyle.Underline));
                                break;
                        }
                    }
                    break;
                case OsCommand.SetClipboard:
                    if (osc.Length > 1 &&
                        (osc[1].Length == 0 || osc[1][0] != '?') &
                        TryParseBase64(osc[1], out ReadOnlySpan<byte> base64) &&
                        TryParseUtf8(base64, out ReadOnlyMemory<char> chars))
                        _setClipboardEvent?.Publish(new SetClipboardEventData(chars));
                    break;
                case OsCommand.ResetColor:
                    if (osc.Length == 0)
                    {
                        for (var i = 0; i < 257; i++)
                            _resetColorEvent?.Publish(new ResetColorEventData((NamedColor)i));
                        break;
                    }

                    for (var i = 0; i < osc.Length; i++)
                    {
                        if (TryParseByte(osc[i], out var index))
                            _resetColorEvent?.Publish(new ResetColorEventData((NamedColor)index));
                    }
                    break;
                case OsCommand.ResetForegroundColor:
                    _resetColorEvent?.Publish(new ResetColorEventData(NamedColor.Foreground));
                    break;
                case OsCommand.ResetBackgroundColor:
                    _resetColorEvent?.Publish(new ResetColorEventData(NamedColor.Background));
                    break;
                case OsCommand.ResetCursorColor:
                    _resetColorEvent?.Publish(new ResetColorEventData(NamedColor.Cursor));
                    break;
            }
            return EventStatus.Continue;
        }

        private EventStatus OnEscapeSequenceEvent(in VT.Events.EscapeSequenceEventData esc)
        {
            int index;
            switch (esc.Command)
            {
                case EscapeCommand.ConfigureAsciiCharSet:
                    if (esc.Intermediates.Length > 0 &&
                        TryParseCharSetIndex(esc.Intermediates.Span[0], out index))
                        _activeCharSets[index] = CharSet.Ascii;
                    break;
                case EscapeCommand.ConfigureSpecialCharSet:
                    if (esc.Intermediates.Length > 0 &&
                        TryParseCharSetIndex(esc.Intermediates.Span[0], out index))
                        _activeCharSets[index] = CharSet.SpecialCharacterAndLineDrawing;
                    break;
                case EscapeCommand.LineFeed:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_lf, 1));
                    break;
                case EscapeCommand.NextLine:
                    _whitespaceEvent?.Publish(new WhitespaceEventData(_crLf, 1));
                    break;
                case EscapeCommand.ReverseIndex:
                    _moveCursorEvent?.Publish(new MoveCursorEventData(MoveOrigin.Inverse, MoveAxis.Row, 1));
                    break;
                case EscapeCommand.IdentifyTerminal:
                    _identifyTerminalEvent?.Publish(new IdentifyTerminalEventData());
                    break;
                case EscapeCommand.ResetState:
                    _stateEvent?.Publish(new StateEventData(StateMode.Reset, States.All));
                    break;
                case EscapeCommand.SaveCursorPosition:
                    _moveCursorEvent?.Publish(new MoveCursorEventData(MoveOrigin.Store, MoveAxis.Row, 0));
                    _moveCursorEvent?.Publish(new MoveCursorEventData(MoveOrigin.Store, MoveAxis.Column, 0));
                    break;
                case EscapeCommand.DecTest: // EscapeCommand.RestoreCusorPosition
                    if (esc.Intermediates.Length != 0 && esc.Intermediates.Span[0] == '#')
                    {
                        // WTF is DECTEST?
                    }
                    else
                    {
                        _moveCursorEvent?.Publish(new MoveCursorEventData(MoveOrigin.Restore, MoveAxis.Row, 0));
                        _moveCursorEvent?.Publish(new MoveCursorEventData(MoveOrigin.Restore, MoveAxis.Column, 0));
                    }
                    break;
                case EscapeCommand.SetKeypadApplicationMode:
                    _stateEvent?.Publish(new StateEventData(StateMode.Set, States.KeypadApplicationMode));
                    break;
                case EscapeCommand.UnsetKeypadApplicationMode:
                    _stateEvent?.Publish(new StateEventData(StateMode.Unset, States.KeypadApplicationMode));
                    break;
            }
            return EventStatus.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<char> MapCharacters(ReadOnlySpan<char> span)
        {
            ref CharSet chars = ref _activeCharSets[_activeCharSetIndex];
            for (var i = 0; i < span.Length; i++)
                _charBuffer[i] = chars[span[i]];
            return _charBuffer.AsMemory(0, span.Length);
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
        private bool TryParseUtf8(ReadOnlySpan<byte> raw, out ReadOnlyMemory<char> result)
        {
            Memory<char> tmp = _charBuffer.AsMemory();
            var count = _utf8.GetChars(raw, tmp.Span);
            result = tmp.Slice(0, count);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryParseColor(ReadOnlySpan<byte> raw, out Color color)
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
        private byte Hex(byte b1, byte b2)
        {
            b1 = Hexit(b1);
            b2 = Hexit(b2);
            b1 <<= 4;
            b1 |= b2;
            return b1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte Hexit(byte value)
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

        private bool TryParseCharSetIndex(byte raw, out int result)
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
    }
}

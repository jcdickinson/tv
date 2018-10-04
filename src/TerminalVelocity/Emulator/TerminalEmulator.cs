using System;
using System.Buffers;
using System.Buffers.Text;
using System.Composition;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using TerminalVelocity.Emulator.Events;
using TerminalVelocity.VT;

namespace TerminalVelocity.Emulator
{
    // https://github.com/jwilm/alacritty/blob/master/src/term/mod.rs
    // https://github.com/jwilm/alacritty/blob/master/src/ansi.rs

    [Shared]
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

        [Import(PrintEvent.ContractName)]
        public Event<PrintEvent> Print { private get; set; }

        [Import(VT.Events.PrintEvent.ContractName)]
        public Event<VT.Events.PrintEvent> OnPrint
        { 
            set => value.Subscribe((ref VT.Events.PrintEvent print) =>
                Print.Publish(new PrintEvent(MapCharacters(print.Characters.Span))));
        }
        
        [Import(Events.WhitespaceEvent.ContractName)]
        public Event<WhitespaceEvent> Whitespace { private get; set; }
        
        [Import(DeleteEvent.ContractName)]
        public Event<DeleteEvent> Delete { private get; set; }
        
        [Import(BellEvent.ContractName)]
        public Event<BellEvent> Bell { private get; set; }
        
        [Import(SetTabstopEvent.ContractName)]
        public Event<SetTabstopEvent> SetTabstop { private get; set; }

        [Import(IdentifyTerminalEvent.ContractName)]
        public Event<IdentifyTerminalEvent> IdentifyTerminal { private get; set; }

        [Import(VT.Events.ExecuteEvent.ContractName)]
        public Event<VT.Events.ExecuteEvent> OnExecute
        { 
            set => value.Subscribe((ref VT.Events.ExecuteEvent execute) =>
            {
                switch (execute.ControlCode)
                {
                    case ControlCode.HorizontalTabulation: 
                        Whitespace.Publish(new WhitespaceEvent(_horizontalTabulator, 1));
                        return;
                    case ControlCode.Backspace:
                        Delete.Publish(new DeleteEvent(Emulator.Delete.Backwards));
                        return;
                    case ControlCode.CarriageReturn:
                        Whitespace.Publish(new WhitespaceEvent(_cr, 1));
                        return;
                    case ControlCode.FormFeed:
                    case ControlCode.VerticalTabulation:
                    case ControlCode.LineFeed:
                        Whitespace.Publish(new WhitespaceEvent(_lf, 1));
                        return;
                    case ControlCode.Bell:
                        Bell.Publish(new BellEvent());
                        return;
                    case ControlCode.ShiftIn:
                        _activeCharSetIndex = G0;
                        return;
                    case ControlCode.ShiftOut:
                        _activeCharSetIndex = G1;
                        return;
                    case ControlCode.NextLine:
                        Whitespace.Publish(new WhitespaceEvent(_crLf, 1));
                        return;
                    case ControlCode.HorizontalTabulationSet:
                        SetTabstop.Publish(new SetTabstopEvent());
                        return;
                    case ControlCode.SingleCharacterIntroducer:
                        IdentifyTerminal.Publish(new IdentifyTerminalEvent());
                        return;
                }
            });
        }

        [Import(SetWindowTitleEvent.ContractName)]
        public Event<SetWindowTitleEvent> SetWindowTitle { private get; set; }

        [Import(SetColorEvent.ContractName)]
        public Event<SetColorEvent> SetColor { private get; set; }

        [Import(SetCursorEvent.ContractName)]
        public Event<SetCursorEvent> SetCursor { private get; set; }

        [Import(SetClipboardEvent.ContractName)]
        public Event<SetClipboardEvent> SetClipboard { private get; set; }

        [Import(ResetColorEvent.ContractName)]
        public Event<ResetColorEvent> ResetColor { private get; set; }
        
        [Import(VT.Events.OsCommandEvent.ContractName)]
        public Event<VT.Events.OsCommandEvent> OnOsCommand
        { 
            set => value.Subscribe((ref VT.Events.OsCommandEvent osc) =>
            {
                if (osc.Parameters.Length == 0 ||
                    !TryParseByte(osc.Parameters.Span[0].Span, out var command))
                    return;

                Color color;
                switch ((OsCommand)command)
                {
                    case OsCommand.SetWindowIconAndTitle:
                    case OsCommand.SetWindowTitle:
                        // Set window title.
                        if (osc.Length > 1 &&
                            TryParseUtf8(osc[1], out var characters))
                            SetWindowTitle.Publish(new SetWindowTitleEvent(characters));
                        return;
                    case OsCommand.SetWindowIcon: return;
                    case OsCommand.SetColor:
                        // Set color index.
                        if (osc.Length % 2 == 0)
                            return;
                        for (var i = 1; i < osc.Length; i++)
                        {
                            if (TryParseByte(osc[i], out var index) &&
                                TryParseColor(osc[i + 1], out color))
                                SetColor.Publish(new SetColorEvent((NamedColor)index, color));
                        }
                        return;
                    case OsCommand.SetForegroundColor:
                        if (osc.Length > 1 &&
                            TryParseColor(osc[1], out color))
                            SetColor.Publish(new SetColorEvent(NamedColor.Foreground, color));
                        return;
                    case OsCommand.SetBackgroundColor:
                        if (osc.Length > 1 &&
                            TryParseColor(osc[1], out color))
                            SetColor.Publish(new SetColorEvent(NamedColor.Background, color));
                        return;
                    case OsCommand.SetCursorColor:
                        if (osc.Length > 1 &&
                            TryParseColor(osc[1], out color))
                            SetColor.Publish(new SetColorEvent(NamedColor.Cursor, color));
                        return;
                    case OsCommand.SetCursorStyle:
                        if (osc.Length > 1 && osc[1].Length > 12 &&
                            osc[1].Slice(0, _cursorShape.Length).SequenceEqual(_cursorShape))
                        {
                            switch((CursorStyle) osc[1][12])
                            {
                                case CursorStyle.Beam:
                                    SetCursor.Publish(new SetCursorEvent(CursorStyle.Beam));
                                    return;
                                case CursorStyle.Block:
                                    SetCursor.Publish(new SetCursorEvent(CursorStyle.Block));
                                    return;
                                case CursorStyle.Underline:
                                    SetCursor.Publish(new SetCursorEvent(CursorStyle.Underline));
                                    return;
                            }
                        }
                        return;
                    case OsCommand.SetClipboard:
                        if (osc.Length > 2 &&
                            (osc[3].Length == 0 || osc[3][0] != '?') &
                            TryParseBase64(osc[3], out var base64) &&
                            TryParseUtf8(base64, out var chars))
                            SetClipboard.Publish(new SetClipboardEvent(chars));
                        return;
                    case OsCommand.ResetColor:
                        if (osc.Length == 1)
                        {
                            for (var i = 0; i < 257; i++)
                                ResetColor.Publish(new ResetColorEvent((NamedColor)i));
                            return;
                        }

                        for (var i = 1; i < osc.Length; i++)
                        {
                            if (TryParseByte(osc[i], out var index))
                                ResetColor.Publish(new ResetColorEvent((NamedColor)index));
                        }
                        return;
                    case OsCommand.ResetForegroundColor:
                        ResetColor.Publish(new ResetColorEvent(NamedColor.Foreground));
                        return;
                    case OsCommand.ResetBackgroundColor:
                        ResetColor.Publish(new ResetColorEvent(NamedColor.Background));
                        return;
                    case OsCommand.ResetCursorColor:
                        ResetColor.Publish(new ResetColorEvent(NamedColor.Cursor));
                        return;
                }
            });
        }

        
        [Import(VT.Events.EscapeSequenceEvent.ContractName)]
        public Event<VT.Events.EscapeSequenceEvent> OnEscapeSequence
        { 
            set => value.Subscribe((ref VT.Events.EscapeSequenceEvent esc) =>
            {
                int index;
                switch((EscapeCommand)esc.Byte)
                {
                    case EscapeCommand.ConfigureAsciiCharSet:
                        if (esc.Intermediates.Length > 0 &&
                            TryParseCharSetIndex(esc.Intermediates.Span[0], out index))
                            _activeCharSets[index] = CharSet.Ascii;
                        return;
                    case EscapeCommand.ConfigureSpecialCharacterAndLineDrawingCharSet:
                        if (esc.Intermediates.Length > 0 &&
                            TryParseCharSetIndex(esc.Intermediates.Span[0], out index))
                            _activeCharSets[index] = CharSet.SpecialCharacterAndLineDrawing;
                        return;
                }
            });
        }

        private readonly char[] _charBuffer;
        private readonly byte[] _byteBuffer;
        private readonly CharSet[] _activeCharSets;
        private int _activeCharSetIndex;
        private readonly UTF8Encoding _utf8;

        public TerminalEmulator(
            int maxCharacters = 4096,
            int maxBytes = 4096)
        {
            if (maxCharacters < 0) throw new ArgumentOutOfRangeException(nameof(maxCharacters));
            if (maxBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));

            _activeCharSets = new[]
            {
                CharSet.Ascii,
                CharSet.Ascii,
                CharSet.Ascii,
                CharSet.Ascii
            };
            _activeCharSetIndex = 0;
            _charBuffer = new char[maxCharacters];
            _byteBuffer = new byte[maxBytes];
            _utf8 = new UTF8Encoding(false, false);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<char> MapCharacters(ReadOnlySpan<char> span)
        {
            ref var chars = ref _activeCharSets[_activeCharSetIndex];
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
            var tmp = _charBuffer.AsMemory();
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
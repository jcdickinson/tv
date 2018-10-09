using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace TerminalVelocity.Terminal
{
    public static class AnsiParserTests
    {
        private static void DefaultHandler<T>(T e) where T : struct { }

        [Fact]
        public static void AnsiParser_Print_ConfigureCharSet_ShiftIn_ShiftOut()
        {
            const string Case = "Hello 😁 world a ";
            const string SpecialCase = "H\n┌┌⎺ 😁 ┬⎺⎼┌\r ▒ ";
            
            var sb = new StringBuilder();
            var ix = 0;
            var current = Case;
            var events = new AnsiParserEvents()
            {
                Input = (characters) =>
                {
                    sb.Append(new string(characters));
                    ix = (ix + characters.Length) % Case.Length;
                }
            };

            var chars = CharacterParser.Create();
            var sut = new AnsiParser(events: events, utf8: chars.TryParseUtf8);

            sut.OnPrint(Case);
            sut.OnPrint(Case);
            Assert.Equal(current + current, sb.ToString());

            // SET G0

            sut.OnEscapeSequenceEvent(EscapeCommand.ConfigureSpecialCharSet, new[] { (byte)'(' });
            sb.Clear();
            ix = 0;
            current = SpecialCase;

            sut.OnPrint(Case);
            sut.OnPrint(Case);
            Assert.Equal(current + current, sb.ToString());

            // Shift

            sut.OnExecute(ControlCode.ShiftOut);
            sb.Clear();
            ix = 0;
            current = Case;

            sut.OnPrint(Case);
            sut.OnPrint(Case);
            Assert.Equal(current + current, sb.ToString());

            // Reset G0 and Shift
            sut.OnEscapeSequenceEvent(EscapeCommand.ConfigureAsciiCharSet, new[] { (byte)'(' });
            sut.OnExecute(ControlCode.ShiftIn);
            sb.Clear();
            ix = 0;
            current = Case;

            sut.OnPrint(Case);
            sut.OnPrint(Case);
            Assert.Equal(current + current, sb.ToString());
        }

        [Fact]
        public static void AnsiParser_Execute_HorizontalTabulation()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                PutTab = (count) =>
                {
                    dispatched++;
                    Assert.Equal(1, count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.HorizontalTabulation);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_CarriageReturn()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                CarriageReturn = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.CarriageReturn);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_FormFeed()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                LineFeed = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.FormFeed);
            sut.OnExecute(ControlCode.VerticalTabulation);
            sut.OnExecute(ControlCode.LineFeed);
            Assert.Equal(3, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_NewLine()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                NewLine = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.NewLine);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_Backspace()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                Backspace = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.Backspace);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_Bell()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                Bell = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.Bell);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_HorizontalTabulationSet()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetHorizontalTabStop = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.HorizontalTabulationSet);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void AnsiParser_Execute_SingleCharacterIntroducer()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                IdentifyTerminal = () =>
                {
                    dispatched++;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnExecute(ControlCode.SingleCharacterIntroducer);
            Assert.Equal(1, dispatched);
        }

        private static void Osc(AnsiParser sut, OsCommand command, params string[] args)
        {
            var arr = new ReadOnlyMemory<byte>[args.Length];
            for (var i = 0; i < args.Length; i++)
                arr[i] = Encoding.UTF8.GetBytes(args[i]);
            sut.OnOsCommand(command, arr.AsSpan());
        }

        [Fact]
        public static void AnsiParser_OsCommand_SetWindowTitle()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetTitle = (characters) =>
                {
                    dispatched++;
                    Assert.Equal("Hello 😁 world", new string(characters));
                }
            };

            var chars = CharacterParser.Create();
            var sut = new AnsiParser(events: events, chars.TryParseUtf8);
            Osc(sut, OsCommand.SetWindowTitle, "Hello 😁 world");
            Osc(sut, OsCommand.SetWindowIconAndTitle, "Hello 😁 world");
            Assert.Equal(2, dispatched);
        }

        [Theory]
        [InlineData(1, NamedColor.Black, 0, 0, 0, OsCommand.SetColor, "0", "#000000")]
        [InlineData(1, NamedColor.Red, 255, 0, 0, OsCommand.SetColor, "1", "#FF0000")]
        [InlineData(1, NamedColor.Green, 0, 255, 0, OsCommand.SetColor, "2", "#00FF00")]
        [InlineData(1, NamedColor.Yellow, 255, 255, 0, OsCommand.SetColor, "3", "#FFFF00")]
        [InlineData(1, NamedColor.Blue, 0, 0, 255, OsCommand.SetColor, "4", "#0000FF")]
        [InlineData(1, NamedColor.Black, 0, 0, 0, OsCommand.SetColor, "0", "rgb:00/00/00")]
        [InlineData(1, NamedColor.Red, 255, 0, 0, OsCommand.SetColor, "1", "rgb:FF/00/00")]
        [InlineData(1, NamedColor.Green, 0, 255, 0, OsCommand.SetColor, "2", "rgb:00/FF/00")]
        [InlineData(1, NamedColor.Yellow, 255, 255, 0, OsCommand.SetColor, "3", "rgb:FF/FF/00")]
        [InlineData(1, NamedColor.Blue, 0, 0, 255, OsCommand.SetColor, "4", "rgb:00/00/FF")]

        [InlineData(0, NamedColor.Blue, 0, 0, 0, OsCommand.SetColor, "4", "rgb")]
        [InlineData(0, NamedColor.Blue, 0, 0, 0, OsCommand.SetColor, "4", "rgb:00!00/FF")]
        [InlineData(0, NamedColor.Blue, 0, 0, 0, OsCommand.SetColor, "4", "!000000")]

        [InlineData(1, NamedColor.Foreground, 85, 170, 255, OsCommand.SetForegroundColor, "#55AAFF")]
        [InlineData(1, NamedColor.Foreground, 85, 170, 255, OsCommand.SetForegroundColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Foreground, 85, 170, 255, OsCommand.SetForegroundColor, "rgb!55/AA/FF")]

        [InlineData(1, NamedColor.Background, 85, 170, 255, OsCommand.SetBackgroundColor, "#55AAFF")]
        [InlineData(1, NamedColor.Background, 85, 170, 255, OsCommand.SetBackgroundColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Background, 85, 170, 255, OsCommand.SetBackgroundColor, "rgb!55/AA/FF")]

        [InlineData(1, NamedColor.Cursor, 85, 170, 255, OsCommand.SetCursorColor, "#55AAFF")]
        [InlineData(1, NamedColor.Cursor, 85, 170, 255, OsCommand.SetCursorColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Cursor, 85, 170, 255, OsCommand.SetCursorColor, "rgb!55/AA/FF")]
        internal static void AnsiParser_OsCommand_SetColor(
            int expectedCount, NamedColor expectedIndex, int expectedR, int expectedG, int expectedB,
            OsCommand command, params string[] param
        )
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetColor = (NamedColor index, in Color color) =>
                {
                    dispatched++;
                    Assert.Equal(expectedIndex, index);
                    Assert.Equal(Color.FromArgb(expectedR, expectedG, expectedB), color);
                }
            };

            var sut = new AnsiParser(events: events);
            Osc(sut, command, param);
            Assert.Equal(expectedCount, dispatched);
        }

        [Fact]
        internal static void AnsiParser_OsCommand_ResetColor_All()
        {
            var expected = new HashSet<NamedColor>(Enumerable.Range(0, 257).Select(x => (NamedColor)x));
            var events = new AnsiParserEvents
            {
                ResetColor = (index) =>
                {
                    Assert.Contains(index, expected);
                    expected.Remove(index);
                }
            };

            var sut = new AnsiParser(events: events);
            Osc(sut, OsCommand.ResetColor);
            Assert.Empty(expected);
        }

        [Fact]
        internal static void AnsiParser_OsCommand_ResetColor_Values()
        {
            var expected = new HashSet<NamedColor>
            {
                NamedColor.Black, NamedColor.Red, NamedColor.Green,
                NamedColor.Yellow, NamedColor.Blue
            };

            var events = new AnsiParserEvents
            {
                ResetColor = (index) =>
                {
                    Assert.Contains(index, expected);
                    expected.Remove(index);
                }
            };

            var sut = new AnsiParser(events: events);
            Osc(sut, OsCommand.ResetColor, "0", "1", "2", "3", "4");
            Assert.Empty(expected);
        }

        [Theory]
        [InlineData(OsCommand.ResetForegroundColor, NamedColor.Foreground)]
        [InlineData(OsCommand.ResetBackgroundColor, NamedColor.Background)]
        [InlineData(OsCommand.ResetCursorColor, NamedColor.Cursor)]
        internal static void AnsiParser_OsCommand_ResetColor_Special(OsCommand command, NamedColor expectedIndex)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ResetColor = (index) =>
                {
                    ++dispatched;
                    Assert.Equal(expectedIndex, index);
                }
            };

            var sut = new AnsiParser(events: events);
            
            Osc(sut, command);
            Assert.Equal(1, dispatched);
        }

        [Theory]
        [InlineData(CursorStyle.Block, "CursorShape=0")]
        [InlineData(CursorStyle.Beam, "CursorShape=1")]
        [InlineData(CursorStyle.Underline, "CursorShape=2")]
        internal static void AnsiParser_OsCommand_SetCursorStyle(
            CursorStyle expectedStyle, params string[] param
        )
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetCursor = (style) =>
                {
                    ++dispatched;
                    Assert.Equal(expectedStyle, style);
                }
            };

            var sut = new AnsiParser(events: events);
            
            Osc(sut, OsCommand.SetCursorStyle, param);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_OsCommand_SetClipboard()
        {
            const string Case = "Hello 😁 world";

            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetClipboard = (characters) =>
                {
                    ++dispatched;
                    Assert.Equal(Case, new string(characters));
                }
            };

            var chars = CharacterParser.Create();
            var sut = new AnsiParser(events: events, utf8: chars.TryParseUtf8);

            var param = Convert.ToBase64String(Encoding.UTF8.GetBytes(Case));
            Osc(sut, OsCommand.SetClipboard, "", param);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_LineFeed()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                LineFeed = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.LineFeed, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_NextLine()
        {
            var crDispatched = 0;
            var lfDispatched = 0;
            var events = new AnsiParserEvents
            {
                CarriageReturn = () =>
                {
                    ++crDispatched;
                },
                LineFeed = () =>
                {
                    ++lfDispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.NextLine, default);
            Assert.Equal(1, crDispatched);
            Assert.Equal(1, lfDispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_ReverseIndex()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ReverseIndex = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.ReverseIndex, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_IdentifyTerminal()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                IdentifyTerminal = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.IdentifyTerminal, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_ResetState()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ResetState = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.ResetState, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_SaveCursorPosition()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SaveCursorPosition = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.SaveCursorPosition, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_RestoreCursorPosition()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                RestoreCursorPosition = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.RestoreCursorPosition, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_SetKeypadApplicationMode()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetKeypadApplicationMode = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.SetKeypadApplicationMode, default);
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void AnsiParser_EscapeSequence_UnsetKeypadApplicationMode()
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                UnsetKeypadApplicationMode = () =>
                {
                    ++dispatched;
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnEscapeSequenceEvent(EscapeCommand.UnsetKeypadApplicationMode, default);
            Assert.Equal(1, dispatched);
        }
    }
}

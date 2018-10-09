/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Xunit;

namespace TerminalVelocity.Terminal
{
    public static class AnsiParserTests
    {
        #region Charset
        [Fact, Trait("Category", "ESC")]
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
        #endregion

        #region Execute
        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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

        [Fact, Trait("Category", "EXEC")]
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
        #endregion

        #region OSC

        private static void Osc(AnsiParser sut, OsCommand command, params string[] args)
        {
            var arr = new ReadOnlyMemory<byte>[args.Length];
            for (var i = 0; i < args.Length; i++)
                arr[i] = Encoding.UTF8.GetBytes(args[i]);
            sut.OnOsCommand(command, arr.AsSpan());
        }

        [Fact, Trait("Category", "OSC")]
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

        [Theory, Trait("Category", "OSC")]
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

        [Fact, Trait("Category", "OSC")]
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

        [Fact, Trait("Category", "OSC")]
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

        [Theory, Trait("Category", "OSC")]
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

        [Theory, Trait("Category", "OSC")]
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

        [Fact, Trait("Category", "OSC")]
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

        #endregion

        #region ESC

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        [Fact, Trait("Category", "ESC")]
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

        #endregion

        #region CSI

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.InsertBlank, new long[] { })]
        [InlineData(ControlSequenceCommand.InsertBlank, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_InsertBlank(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                InsertBlank = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveUp, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveUp, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveUp(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveUp = (count, cr) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.False(cr);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Fact, Trait("Category", "CSI")]
        internal static void AnsiParser_ControlSequence_RepeatPrecedingCharacter()
        {
            var sb = new StringBuilder();
            var events = new AnsiParserEvents
            {
                Input = (str) =>
                {
                    sb.Append(new string(str));
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnPrint("Hello");
            sut.OnControlSequenceCommand(ControlSequenceCommand.RepeatPrecedingCharacter, default, new long[] { 5 });
            Assert.Equal("Helloooooo", sb.ToString());
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveDown1, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveDown1, new long[] { 100 })]
        [InlineData(ControlSequenceCommand.MoveDown2, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveDown2, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveDown(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveDown = (count, cr) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.False(cr);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Fact, Trait("Category", "CSI")]
        internal static void AnsiParser_ControlSequence_IdentifyTerminal()
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
            sut.OnControlSequenceCommand(ControlSequenceCommand.IdentifyTerminal, default, default);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveForward1, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveForward1, new long[] { 100 })]
        [InlineData(ControlSequenceCommand.MoveForward2, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveForward2, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveForward(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveForward = (count, tabs) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.False(tabs);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveBackward, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveBackward, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveBackward(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveBackward = (count, tabs) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.False(tabs);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveDownAndCr, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveDownAndCr, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveDownAndCr(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveDown = (count, cr) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.True(cr);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveUpAndCr, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveUpAndCr, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveUpAndCr(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveUp = (count, cr) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.True(cr);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.ClearTabulation, new long[] { })]
        [InlineData(ControlSequenceCommand.ClearTabulation, new long[] { 0 })]
        [InlineData(ControlSequenceCommand.ClearTabulation, new long[] { 3 })]
        internal static void AnsiParser_ControlSequence_ClearTabulation(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ClearTabulation = (mode) =>
                {
                    ++dispatched;
                    Assert.Equal((TabulationClearMode)param.AsSpan().Optional(0).GetValueOrDefault(0), mode);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.GotoColumn1, new long[] { })]
        [InlineData(ControlSequenceCommand.GotoColumn1, new long[] { 100 })]
        [InlineData(ControlSequenceCommand.GotoColumn2, new long[] { })]
        [InlineData(ControlSequenceCommand.GotoColumn2, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_GotoColumn(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                Goto = (column, row) =>
                {
                    ++dispatched;
                    Assert.False(row.HasValue);
                    Assert.True(column.HasValue);
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1) - 1, column.Value);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.Goto1, new long[] { })]
        [InlineData(ControlSequenceCommand.Goto1, new long[] { 100 })]
        [InlineData(ControlSequenceCommand.Goto1, new long[] { 100, 200 })]
        [InlineData(ControlSequenceCommand.Goto2, new long[] { })]
        [InlineData(ControlSequenceCommand.Goto2, new long[] { 100 })]
        [InlineData(ControlSequenceCommand.Goto2, new long[] { 100, 200 })]
        internal static void AnsiParser_ControlSequence_Goto(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                Goto = (column, row) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1) - 1, column.Value);
                    Assert.Equal(param.AsSpan().Optional(1).GetValueOrDefault(1) - 1, row.Value);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveForwardTabs, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveForwardTabs, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveForwardTabs(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveForward = (count, tabs) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.True(tabs);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.ClearScreen, new long[] { })]
        [InlineData(ControlSequenceCommand.ClearScreen, new long[] { 0 })]
        [InlineData(ControlSequenceCommand.ClearScreen, new long[] { 1 })]
        [InlineData(ControlSequenceCommand.ClearScreen, new long[] { 2 })]
        [InlineData(ControlSequenceCommand.ClearScreen, new long[] { 3 })]
        internal static void AnsiParser_ControlSequence_ClearScreen(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ClearScreen = (mode) =>
                {
                    ++dispatched;
                    Assert.Equal((ClearMode)param.AsSpan().Optional(0).GetValueOrDefault(0), mode);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.ClearLine, new long[] { })]
        [InlineData(ControlSequenceCommand.ClearLine, new long[] { 0 })]
        [InlineData(ControlSequenceCommand.ClearLine, new long[] { 1 })]
        [InlineData(ControlSequenceCommand.ClearLine, new long[] { 2 })]
        internal static void AnsiParser_ControlSequence_ClearLine(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ClearLine = (mode) =>
                {
                    ++dispatched;
                    Assert.Equal((LineClearMode)param.AsSpan().Optional(0).GetValueOrDefault(0), mode);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.ScrollUp, new long[] { })]
        [InlineData(ControlSequenceCommand.ScrollUp, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_ScrollUp(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ScrollUp = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.ScrollDown, new long[] { })]
        [InlineData(ControlSequenceCommand.ScrollDown, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_ScrollDown(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                ScrollDown = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.InsertBlankLines, new long[] { })]
        [InlineData(ControlSequenceCommand.InsertBlankLines, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_InsertBlankLines(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                InsertBlankLines = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.UnsetMode, new long[] { })]
        [InlineData(ControlSequenceCommand.UnsetMode, new long[] { 1 })]
        [InlineData(ControlSequenceCommand.UnsetMode, new long[] { 1, 3, 4, 6 })]
        internal static void AnsiParser_ControlSequence_UnsetMode(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                UnsetMode = (mode) =>
                {
                    Assert.Equal((Mode)param.AsSpan().Optional(dispatched++).GetValueOrDefault(0), mode);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(param.Length, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.DeleteLines, new long[] { })]
        [InlineData(ControlSequenceCommand.DeleteLines, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_DeleteLines(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                DeleteLines = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.EraseChars, new long[] { })]
        [InlineData(ControlSequenceCommand.EraseChars, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_EraseChars(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                EraseCharacters = (count) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.MoveBackwardTabs, new long[] { })]
        [InlineData(ControlSequenceCommand.MoveBackwardTabs, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_MoveBackwardTabs(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                MoveBackward = (count, tabs) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1), count);
                    Assert.True(tabs);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.GotoLine, new long[] { })]
        [InlineData(ControlSequenceCommand.GotoLine, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_GotoLine(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                Goto = (column, line) =>
                {
                    ++dispatched;
                    Assert.False(column.HasValue);
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(1) - 1, line);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.SetMode, new long[] { })]
        [InlineData(ControlSequenceCommand.SetMode, new long[] { 1 })]
        [InlineData(ControlSequenceCommand.SetMode, new long[] { 1, 3, 4, 6 })]
        internal static void AnsiParser_ControlSequence_SetMode(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetMode = (mode) =>
                {
                    Assert.Equal((Mode)param.AsSpan().Optional(dispatched++).GetValueOrDefault(0), mode);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(param.Length, dispatched);
        }

        public static IEnumerable<object[]> AnsiParser_ControlSequence_TerminalAttributeData
        {
            get
            {
                for (var i = 0; i < 30; i++)
                {
                    yield return new object[]
                    {
                        ControlSequenceCommand.TerminalAttribute, new[] { (long)i },
                        (TerminalAttribute)i, new NamedColor?(), new Color?()
                    };
                }

                for (var i = 30; i < 38; i++)
                {
                    var index = (NamedColor)(i - 30);
                    yield return new object[]
                    {
                        ControlSequenceCommand.TerminalAttribute, new[] { (long)i },
                        TerminalAttribute.SetForeground, new NamedColor?(index), new Color?()
                    };
                }

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 38L, 2L, 255L, 255L, 0L },
                    TerminalAttribute.SetForeground, new NamedColor?(), new Color?(Color.FromArgb(255, 255, 0))
                };

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 38L, 5L, (long)NamedColor.Yellow },
                    TerminalAttribute.SetForeground, new NamedColor?(NamedColor.Yellow), new Color?()
                };

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 39L },
                    TerminalAttribute.SetForeground, new NamedColor?(NamedColor.Foreground), new Color?()
                };

                for (var i = 40; i < 48; i++)
                {
                    var index = (NamedColor)(i - 40);
                    yield return new object[]
                    {
                        ControlSequenceCommand.TerminalAttribute, new[] { (long)i },
                        TerminalAttribute.SetForeground, new NamedColor?(index), new Color?()
                    };
                }

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 48L, 2L, 255L, 255L, 0L },
                    TerminalAttribute.SetBackground, new NamedColor?(), new Color?(Color.FromArgb(255, 255, 0))
                };

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 48L, 5L, (long)NamedColor.Yellow },
                    TerminalAttribute.SetBackground, new NamedColor?(NamedColor.Yellow), new Color?()
                };

                yield return new object[]
                {
                    ControlSequenceCommand.TerminalAttribute, new[] { 49L },
                    TerminalAttribute.SetBackground, new NamedColor?(NamedColor.Background), new Color?()
                };

                for (var i = 90; i < 98; i++)
                {
                    var index = (NamedColor)(i - 90 + (int)NamedColor.BrightBlack);
                    yield return new object[]
                    {
                        ControlSequenceCommand.TerminalAttribute, new[] { (long)i },
                        TerminalAttribute.SetForeground, new NamedColor?(index), new Color?()
                    };
                }

                for (var i = 100; i < 108; i++)
                {
                    var index = (NamedColor)(i - 100 + (int)NamedColor.BrightBlack);
                    yield return new object[]
                    {
                        ControlSequenceCommand.TerminalAttribute, new[] { (long)i },
                        TerminalAttribute.SetBackground, new NamedColor?(index), new Color?()
                    };
                }
            }
        }

        [Theory, Trait("Category", "CSI")]
        [MemberData(nameof(AnsiParser_ControlSequence_TerminalAttributeData))]
        internal static void AnsiParser_ControlSequence_TerminalAttribute(
            ControlSequenceCommand command, long[] param,
            TerminalAttribute expectedAttribute, NamedColor? expectedIndex, Color? expectedColor
            )
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                TerminalAttribute = (TerminalAttribute attribute, NamedColor? index, in Color? color) =>
                {
                    ++dispatched;
                    Assert.Equal(expectedAttribute, attribute);
                    Assert.Equal(expectedIndex, index);
                    Assert.Equal(expectedColor, color);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.DeviceStatus, new long[] { })]
        [InlineData(ControlSequenceCommand.DeviceStatus, new long[] { 100 })]
        internal static void AnsiParser_ControlSequence_DeviceStatus(ControlSequenceCommand command, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                DeviceStatus = (p) =>
                {
                    ++dispatched;
                    Assert.Equal(param.AsSpan().Optional(0).GetValueOrDefault(0), p);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.SaveCursorPosition, new long[] { })]
        internal static void AnsiParser_ControlSequence_SaveCursorPosition(ControlSequenceCommand command, long[] param)
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
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.RestoreCursorPosition, new long[] { })]
        internal static void AnsiParser_ControlSequence_RestoreCursorPosition(ControlSequenceCommand command, long[] param)
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
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }

        [Theory, Trait("Category", "CSI")]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Beam, new long[] { })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Block, new long[] { 1 })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Block, new long[] { 2 })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Underline, new long[] { 3 })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Underline, new long[] { 4 })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Beam, new long[] { 5 })]
        [InlineData(ControlSequenceCommand.SetCursorStyle, CursorStyle.Beam, new long[] { 6 })]
        internal static void AnsiParser_ControlSequence_SetCursorStyle(ControlSequenceCommand command, CursorStyle cursorStyle, long[] param)
        {
            var dispatched = 0;
            var events = new AnsiParserEvents
            {
                SetCursorStyle = (style) =>
                {
                    ++dispatched;
                    if (param.Length == 0)
                        Assert.False(style.HasValue);
                    else
                        Assert.Equal(cursorStyle, style);
                }
            };

            var sut = new AnsiParser(events: events);
            sut.OnControlSequenceCommand(command, default, param);
            Assert.Equal(1, dispatched);
        }



        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using TerminalVelocity.Emulator.Events;
using TerminalVelocity.Eventing;
using TerminalVelocity.VT;
using Xunit;

namespace TerminalVelocity.Emulator
{
    public static class TerminalEmulatorTests
    {
        private static void DefaultHandler<T>(T e) where T : struct { }

        [Fact]
        public static void TerminalEmulator_Print_ConfigureCharSet_ShiftIn_ShiftOut()
        {
            const string Case = "Hello 😁 world a ";
            const string SpecialCase = "H\n┌┌⎺ 😁 ┬⎺⎼┌\r ▒ ";

            var onPrint = new VT.Events.PrintEvent(DefaultHandler);
            var onEsc = new VT.Events.EscapeSequenceEvent(DefaultHandler);
            var onExec = new VT.Events.ExecuteEvent(DefaultHandler);

            var sb = new StringBuilder();
            var ix = 0;
            var current = Case;
            var sut = new TerminalEmulator(
                onPrintEvent: onPrint, onEscapeSequenceEvent: onEsc, onExecuteEvent: onExec,
                printEvent: new PrintEvent(print =>
                {
                    sb.Append(new string(print.Characters.Span));
                    Assert.Equal(current.Substring(ix, print.Characters.Length), print.ToString());
                    ix = (ix + print.Characters.Length) % Case.Length;
                }));

            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // SET G0

            onEsc.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.ConfigureSpecialCharSet, new byte[] { (byte)'(' }, VT.IgnoredData.None));
            sb.Clear();
            ix = 0;
            current = SpecialCase;

            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // Shift

            onExec.Publish(new VT.Events.ExecuteEventData(VT.ControlCode.ShiftOut));
            sb.Clear();
            ix = 0;
            current = Case;

            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // Reset G0 and Shift
            onEsc.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.ConfigureAsciiCharSet, new byte[] { (byte)'(' }, VT.IgnoredData.None));
            onExec.Publish(new VT.Events.ExecuteEventData(VT.ControlCode.ShiftIn));
            sb.Clear();
            ix = 0;
            current = Case;

            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEventData(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());
        }

        [Fact]
        public static void TerminalEmulator_Execute_HorizontalTabulation()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    ++dispatched;
                    Assert.Equal("\t".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\t*1", ws.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.HorizontalTabulation));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_CarriageReturn()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    ++dispatched;
                    Assert.Equal("\r".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\r*1", ws.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.CarriageReturn));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_FormFeed()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    ++dispatched;
                    Assert.Equal("\n".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\n*1", ws.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.FormFeed));
            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.VerticalTabulation));
            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.LineFeed));
            Assert.Equal(3, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_NextLine()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    ++dispatched;
                    Assert.Equal("\r\n".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\r\n*1", ws.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.NextLine));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_Backspace()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                deleteEvent: new DeleteEvent(delete =>
                {
                    ++dispatched;
                    Assert.Equal(DeleteDirection.Backwards, delete.Direction);
                    Assert.Equal("Backwards", delete.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.Backspace));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_Bell()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                bellEvent: new BellEvent(bell =>
                {
                    ++dispatched;
                    Assert.Equal(string.Empty, bell.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.Bell));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_HorizontalTabulationSet()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                setTabstopEvent: new SetTabstopEvent(sts =>
                {
                    ++dispatched;
                    Assert.Equal(string.Empty, sts.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.HorizontalTabulationSet));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        public static void TerminalEmulator_Execute_SingleCharacterIntroducer()
        {
            var onExecute = new VT.Events.ExecuteEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onExecuteEvent: onExecute,
                identifyTerminalEvent: new IdentifyTerminalEvent(id =>
                {
                    ++dispatched;
                    Assert.Equal(string.Empty, id.ToString());
                }));

            onExecute.Publish(new VT.Events.ExecuteEventData(ControlCode.SingleCharacterIntroducer));
            Assert.Equal(1, dispatched);
        }

        private static VT.Events.OsCommandEventData Osc(OsCommand command, params string[] args)
        {
            var arr = new ReadOnlyMemory<byte>[args.Length + 1];
            arr[0] = Encoding.ASCII.GetBytes(((int)command).ToString(CultureInfo.InvariantCulture));
            for (var i = 0; i < args.Length; i++)
                arr[i + 1] = Encoding.UTF8.GetBytes(args[i]);
            return new VT.Events.OsCommandEventData(arr, IgnoredData.None);
        }

        [Fact]
        public static void TerminalEmulator_OsCommand_SetWindowTitle()
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                setWindowTitleEvent: new SetWindowTitleEvent(title =>
                {
                    ++dispatched;
                    Assert.Equal("Hello 😁 world".ToCharArray(), title.Characters.ToArray());
                    Assert.Equal("Hello 😁 world", title.ToString());
                }));

            onOsCommand.Publish(Osc(OsCommand.SetWindowTitle, "Hello 😁 world"));
            onOsCommand.Publish(Osc(OsCommand.SetWindowIconAndTitle, "Hello 😁 world"));
            Assert.Equal(2, dispatched);
        }

        [Theory]
        [InlineData(2, NamedColor.Black, 0, 0, 0, "Black=(0,0,0)", OsCommand.SetColor, "0", "#000000")]
        [InlineData(2, NamedColor.Red, 255, 0, 0, "Red=(255,0,0)", OsCommand.SetColor, "1", "#FF0000")]
        [InlineData(2, NamedColor.Green, 0, 255, 0, "Green=(0,255,0)", OsCommand.SetColor, "2", "#00FF00")]
        [InlineData(2, NamedColor.Yellow, 255, 255, 0, "Yellow=(255,255,0)", OsCommand.SetColor, "3", "#FFFF00")]
        [InlineData(2, NamedColor.Blue, 0, 0, 255, "Blue=(0,0,255)", OsCommand.SetColor, "4", "#0000FF")]
        [InlineData(2, NamedColor.Black, 0, 0, 0, "Black=(0,0,0)", OsCommand.SetColor, "0", "rgb:00/00/00")]
        [InlineData(2, NamedColor.Red, 255, 0, 0, "Red=(255,0,0)", OsCommand.SetColor, "1", "rgb:FF/00/00")]
        [InlineData(2, NamedColor.Green, 0, 255, 0, "Green=(0,255,0)", OsCommand.SetColor, "2", "rgb:00/FF/00")]
        [InlineData(2, NamedColor.Yellow, 255, 255, 0, "Yellow=(255,255,0)", OsCommand.SetColor, "3", "rgb:FF/FF/00")]
        [InlineData(2, NamedColor.Blue, 0, 0, 255, "Blue=(0,0,255)", OsCommand.SetColor, "4", "rgb:00/00/FF")]

        [InlineData(0, NamedColor.Blue, 0, 0, 0, "", OsCommand.SetColor, "4", "rgb")]
        [InlineData(0, NamedColor.Blue, 0, 0, 0, "", OsCommand.SetColor, "4", "rgb:00!00/FF")]
        [InlineData(0, NamedColor.Blue, 0, 0, 0, "", OsCommand.SetColor, "4", "!000000")]

        [InlineData(1, NamedColor.Foreground, 85, 170, 255, "Foreground=(85,170,255)", OsCommand.SetForegroundColor, "#55AAFF")]
        [InlineData(1, NamedColor.Foreground, 85, 170, 255, "Foreground=(85,170,255)", OsCommand.SetForegroundColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Foreground, 85, 170, 255, "", OsCommand.SetForegroundColor, "rgb!55/AA/FF")]

        [InlineData(1, NamedColor.Background, 85, 170, 255, "Background=(85,170,255)", OsCommand.SetBackgroundColor, "#55AAFF")]
        [InlineData(1, NamedColor.Background, 85, 170, 255, "Background=(85,170,255)", OsCommand.SetBackgroundColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Background, 85, 170, 255, "", OsCommand.SetBackgroundColor, "rgb!55/AA/FF")]

        [InlineData(1, NamedColor.Cursor, 85, 170, 255, "Cursor=(85,170,255)", OsCommand.SetCursorColor, "#55AAFF")]
        [InlineData(1, NamedColor.Cursor, 85, 170, 255, "Cursor=(85,170,255)", OsCommand.SetCursorColor, "rgb:55/AA/FF")]
        [InlineData(0, NamedColor.Cursor, 85, 170, 255, "", OsCommand.SetCursorColor, "rgb!55/AA/FF")]
        internal static void TerminalEmulator_OsCommand_SetColor(
            int expectedCount, NamedColor expectedIndex, int expectedR, int expectedG, int expectedB, string expectedMessage,
            OsCommand command, params string[] param
        )
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                setColorEvent: new SetColorEvent(color =>
                {
                    Assert.Equal(expectedIndex, color.Index);
                    Assert.Equal(Color.FromArgb(expectedR, expectedG, expectedB), color.Color);
                    Assert.Equal(expectedMessage, color.ToString());
                    dispatched++;
                }));

            param = param.Concat(param).ToArray();
            onOsCommand.Publish(Osc(command, param));
            Assert.Equal(expectedCount, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetColor_All()
        {
            var expected = new HashSet<NamedColor>(Enumerable.Range(0, 257).Select(x => (NamedColor)x));

            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                resetColorEvent: new ResetColorEvent(color =>
                {
                    Assert.Contains(color.Index, expected);
                    expected.Remove(color.Index);
                    Assert.Equal(color.Index.ToString(), color.ToString());
                }));

            onOsCommand.Publish(Osc(OsCommand.ResetColor));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetColor_Values()
        {
            var expected = new HashSet<NamedColor>
            {
                NamedColor.Black, NamedColor.Red, NamedColor.Green,
                NamedColor.Yellow, NamedColor.Blue
            };

            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                resetColorEvent: new ResetColorEvent(color =>
                {
                    Assert.Contains(color.Index, expected);
                    expected.Remove(color.Index);
                    Assert.Equal(color.Index.ToString(), color.ToString());
                }));

            onOsCommand.Publish(Osc(OsCommand.ResetColor, "0", "1", "2", "3", "4"));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetForegroundColor()
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                resetColorEvent: new ResetColorEvent(color =>
                {
                    Assert.Equal(NamedColor.Foreground, color.Index);
                    Assert.Equal("Foreground", color.ToString());
                    dispatched++;
                }));

            onOsCommand.Publish(Osc(OsCommand.ResetForegroundColor));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetBackgroundColor()
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                resetColorEvent: new ResetColorEvent(color =>
                {
                    Assert.Equal(NamedColor.Background, color.Index);
                    Assert.Equal("Background", color.ToString());
                    dispatched++;
                }));

            onOsCommand.Publish(Osc(OsCommand.ResetBackgroundColor));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetCursorColor()
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                resetColorEvent: new ResetColorEvent(color =>
                {
                    Assert.Equal(NamedColor.Cursor, color.Index);
                    Assert.Equal("Cursor", color.ToString());
                    dispatched++;
                }));

            onOsCommand.Publish(Osc(OsCommand.ResetCursorColor));
            Assert.Equal(1, dispatched);
        }

        [Theory]
        [InlineData(CursorStyle.Block, "Block", "CursorShape=0")]
        [InlineData(CursorStyle.Beam, "Beam", "CursorShape=1")]
        [InlineData(CursorStyle.Underline, "Underline", "CursorShape=2")]
        internal static void TerminalEmulator_OsCommand_SetCursorStyle(
            CursorStyle expectedStyle, string expectedMessage, params string[] param
        )
        {
            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                setCursorEvent: new SetCursorEvent(cursor =>
                {
                    Assert.Equal(expectedStyle, cursor.Style);
                    Assert.Equal(expectedMessage, cursor.ToString());
                    dispatched++;
                }));

            onOsCommand.Publish(Osc(OsCommand.SetCursorStyle, param));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_SetClipboard()
        {
            const string Case = "Hello 😁 world";

            var onOsCommand = new VT.Events.OsCommandEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onOsCommandEvent: onOsCommand,
                setClipboardEvent: new SetClipboardEvent(clipboard =>
                {
                    Assert.Equal(Case.ToCharArray(), clipboard.Characters.ToArray());
                    Assert.Equal("Hello 😁 world", clipboard.ToString());
                    dispatched++;
                }));

            var param = Convert.ToBase64String(Encoding.UTF8.GetBytes(Case));
            onOsCommand.Publish(Osc(OsCommand.SetClipboard, "", param));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_LineFeed()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    Assert.Equal("\n".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\n*1", ws.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.LineFeed, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_NextLine()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                whitespaceEvent: new WhitespaceEvent(ws =>
                {
                    Assert.Equal("\r\n".ToCharArray(), ws.Characters.ToArray());
                    Assert.Equal(1, ws.Count);
                    Assert.Equal("\r\n*1", ws.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.NextLine, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_ReverseIndex()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                moveCursorEvent: new MoveCursorEvent(move =>
                {
                    Assert.Equal(MoveOrigin.Inverse, move.Origin);
                    Assert.Equal(MoveAxis.Row, move.Axis);
                    Assert.Equal(1, move.Count);
                    Assert.Equal("Inverse Row 1", move.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.ReverseIndex, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_IdentifyTerminal()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                identifyTerminalEvent: new IdentifyTerminalEvent(id =>
                {
                    Assert.Equal(string.Empty, id.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.IdentifyTerminal, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_ResetState()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                stateEvent: new StateEvent(state =>
                {
                    Assert.Equal(States.All, state.States);
                    Assert.Equal(StateMode.Reset, state.Mode);
                    Assert.Equal("Reset All", state.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.ResetState, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_SaveCursorPosition()
        {
            var expected = new HashSet<MoveAxis>
            {
                MoveAxis.Row, MoveAxis.Column
            };

            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                moveCursorEvent: new MoveCursorEvent(move =>
                {
                    Assert.Contains(move.Axis, expected);
                    expected.Remove(move.Axis);
                    Assert.Equal(MoveOrigin.Store, move.Origin);
                    Assert.Equal(0, move.Count);
                    Assert.Equal(FormattableString.Invariant($"Store {move.Axis} 0"), move.ToString());
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.SaveCursorPosition, default, IgnoredData.None));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_RestoreCursorPosition()
        {
            var expected = new HashSet<MoveAxis>
            {
                MoveAxis.Row, MoveAxis.Column
            };

            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                moveCursorEvent: new MoveCursorEvent(move =>
                {
                    Assert.Contains(move.Axis, expected);
                    expected.Remove(move.Axis);
                    Assert.Equal(MoveOrigin.Restore, move.Origin);
                    Assert.Equal(0, move.Count);
                    Assert.Equal(FormattableString.Invariant($"Restore {move.Axis} 0"), move.ToString());
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.RestoreCursorPosition, default, IgnoredData.None));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_SetKeypadApplicationMode()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                stateEvent: new StateEvent(state =>
                {
                    Assert.Equal(States.KeypadApplicationMode, state.States);
                    Assert.Equal(StateMode.Set, state.Mode);
                    Assert.Equal("Set KeypadApplicationMode", state.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.SetKeypadApplicationMode, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_EscapeSequence_UnsetKeypadApplicationMode()
        {
            var onEscapeSequence = new VT.Events.EscapeSequenceEvent(DefaultHandler);

            var dispatched = 0;
            var sut = new TerminalEmulator(onEscapeSequenceEvent: onEscapeSequence,
                stateEvent: new StateEvent(state =>
                {
                    Assert.Equal(States.KeypadApplicationMode, state.States);
                    Assert.Equal(StateMode.Unset, state.Mode);
                    Assert.Equal("Unset KeypadApplicationMode", state.ToString());
                    dispatched++;
                }));

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEventData(EscapeCommand.UnsetKeypadApplicationMode, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
    }
}

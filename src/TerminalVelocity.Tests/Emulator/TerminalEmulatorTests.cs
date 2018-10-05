using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using TerminalVelocity.Emulator.Events;
using TerminalVelocity.VT;
using Xunit;

namespace TerminalVelocity.Emulator
{
    public static class TerminalEmulatorTests
    {
        [Fact]
        public static void TerminalEmulator_Print_ConfigureCharSet_ShiftIn_ShiftOut()
        {
            const string Case = "Hello 😁 world a ";
            const string SpecialCase = "H\n┌┌⎺ 😁 ┬⎺⎼┌\r ▒ ";

            var sut = new TerminalEmulator();
            var onPrint = new Event<VT.Events.PrintEvent>("VT.Print");
            var onEsc = new Event<VT.Events.EscapeSequenceEvent>("VT.Escape");
            var onExec = new Event<VT.Events.ExecuteEvent>("VT.Execute");
            sut.OnPrint = onPrint;
            sut.OnEscapeSequence = onEsc;
            sut.OnExecute = onExec;

            var sb = new StringBuilder();
            var ix = 0;
            var current = Case;
            sut.Print = new Event<Events.PrintEvent>("Print", print =>
            {
                sb.Append(new string(print.Characters.Span));
                Assert.Equal(current.Substring(ix, print.Characters.Length), print.ToString());
                ix = (ix + print.Characters.Length) % Case.Length;
            });

            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // SET G0

            onEsc.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.ConfigureSpecialCharSet, new byte[] { (byte)'(' }, VT.IgnoredData.None));
            sb.Clear();
            ix = 0;
            current = SpecialCase;

            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // Shift

            onExec.Publish(new VT.Events.ExecuteEvent(VT.ControlCode.ShiftOut));
            sb.Clear();
            ix = 0;
            current = Case;
            
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());

            // Reset G0 and Shift
            onEsc.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.ConfigureAsciiCharSet, new byte[] { (byte)'(' }, VT.IgnoredData.None));
            onExec.Publish(new VT.Events.ExecuteEvent(VT.ControlCode.ShiftIn));
            sb.Clear();
            ix = 0;
            current = Case;
            
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_HorizontalTabulation()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                ++dispatched;
                Assert.Equal("\t".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\t*1", ws.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.HorizontalTabulation));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_CarriageReturn()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                ++dispatched;
                Assert.Equal("\r".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\r*1", ws.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.CarriageReturn));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_FormFeed()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                ++dispatched;
                Assert.Equal("\n".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\n*1", ws.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.FormFeed));
            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.VerticalTabulation));
            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.LineFeed));
            Assert.Equal(3, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_NextLine()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                ++dispatched;
                Assert.Equal("\r\n".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\r\n*1", ws.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.NextLine));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_Backspace()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Delete = new Event<DeleteEvent>("Delete", delete =>
            {
                ++dispatched;
                Assert.Equal(DeleteDirection.Backwards, delete.Direction);
                Assert.Equal("Backwards", delete.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.Backspace));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_Bell()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.Bell = new Event<BellEvent>("Bell", bell =>
            {
                ++dispatched;
                Assert.Equal(string.Empty, bell.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.Bell));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_HorizontalTabulationSet()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.SetTabstop = new Event<SetTabstopEvent>("SetTabstop", sts =>
            {
                ++dispatched;
                Assert.Equal(string.Empty, sts.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.HorizontalTabulationSet));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        public static void TerminalEmulator_Execute_SingleCharacterIntroducer()
        {
            var sut = new TerminalEmulator();
            var onExecute = new Event<VT.Events.ExecuteEvent>("Execute");
            sut.OnExecute = onExecute;

            var dispatched = 0;
            sut.IdentifyTerminal = new Event<IdentifyTerminalEvent>("IdentifyTerminal", id =>
            {
                ++dispatched;
                Assert.Equal(string.Empty, id.ToString());
            });

            onExecute.Publish(new VT.Events.ExecuteEvent(ControlCode.SingleCharacterIntroducer));
            Assert.Equal(1, dispatched);
        }

        private static VT.Events.OsCommandEvent Osc(OsCommand command, params string[] args)
        {
            var arr = new ReadOnlyMemory<byte>[args.Length + 1];
            arr[0] = Encoding.ASCII.GetBytes(((int)command).ToString(CultureInfo.InvariantCulture));
            for (var i = 0; i < args.Length; i++)
                arr[i + 1] = Encoding.UTF8.GetBytes(args[i]);
            return new VT.Events.OsCommandEvent(arr, IgnoredData.None);
        }
        
        [Fact]
        public static void TerminalEmulator_OsCommand_SetWindowTitle()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.SetWindowTitle = new Event<SetWindowTitleEvent>("SetWindowTitle", title =>
            {
                ++dispatched;
                Assert.Equal("Hello 😁 world".ToCharArray(), title.Characters.ToArray());
                Assert.Equal("Hello 😁 world", title.ToString());
            });

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
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.SetColor = new Event<SetColorEvent>("SetColor", color =>
            {
                Assert.Equal(expectedIndex, color.Index);
                Assert.Equal(Color.FromArgb(expectedR, expectedG, expectedB), color.Color);
                Assert.Equal(expectedMessage, color.ToString());
                dispatched++;
            });

            param = param.Concat(param).ToArray();
            onOsCommand.Publish(Osc(command, param));
            Assert.Equal(expectedCount, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetColor_All()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var expected = new HashSet<NamedColor>(Enumerable.Range(0, 257).Select(x => (NamedColor)x));
            sut.ResetColor = new Event<ResetColorEvent>("ResetColor", color =>
            {
                Assert.Contains(color.Index, expected);
                expected.Remove(color.Index);
                Assert.Equal(color.Index.ToString(), color.ToString());
            });

            onOsCommand.Publish(Osc(OsCommand.ResetColor));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetColor_Values()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var expected = new HashSet<NamedColor>
            {
                NamedColor.Black, NamedColor.Red, NamedColor.Green,
                NamedColor.Yellow, NamedColor.Blue
            };
            sut.ResetColor = new Event<ResetColorEvent>("ResetColor", color =>
            {
                Assert.Contains(color.Index, expected);
                expected.Remove(color.Index);
                Assert.Equal(color.Index.ToString(), color.ToString());
            });

            onOsCommand.Publish(Osc(OsCommand.ResetColor, "0", "1", "2", "3", "4"));
            Assert.Empty(expected);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetForegroundColor()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.ResetColor = new Event<ResetColorEvent>("ResetColor", color =>
            {
                Assert.Equal(NamedColor.Foreground, color.Index);
                Assert.Equal("Foreground", color.ToString());
                dispatched++;
            });

            onOsCommand.Publish(Osc(OsCommand.ResetForegroundColor));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetBackgroundColor()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.ResetColor = new Event<ResetColorEvent>("ResetColor", color =>
            {
                Assert.Equal(NamedColor.Background, color.Index);
                Assert.Equal("Background", color.ToString());
                dispatched++;
            });

            onOsCommand.Publish(Osc(OsCommand.ResetBackgroundColor));
            Assert.Equal(1, dispatched);
        }

        [Fact]
        internal static void TerminalEmulator_OsCommand_ResetCursorColor()
        {
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.ResetColor = new Event<ResetColorEvent>("ResetColor", color =>
            {
                Assert.Equal(NamedColor.Cursor, color.Index);
                Assert.Equal("Cursor", color.ToString());
                dispatched++;
            });

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
            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.SetCursor = new Event<SetCursorEvent>("SetCursor", cursor =>
            {
                Assert.Equal(expectedStyle, cursor.Style);
                Assert.Equal(expectedMessage, cursor.ToString());
                dispatched++;
            });

            onOsCommand.Publish(Osc(OsCommand.SetCursorStyle, param));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_OsCommand_SetClipboard()
        {
            const string Case = "Hello 😁 world";

            var sut = new TerminalEmulator();
            var onOsCommand = new Event<VT.Events.OsCommandEvent>("OsCommand");
            sut.OnOsCommand = onOsCommand;

            var dispatched = 0;
            sut.SetClipboard = new Event<SetClipboardEvent>("SetClipboard", clipboard =>
            {
                Assert.Equal(Case.ToCharArray(), clipboard.Characters.ToArray());
                Assert.Equal("Hello 😁 world", clipboard.ToString());
                dispatched++;
            });

            var param = Convert.ToBase64String(Encoding.UTF8.GetBytes(Case));
            onOsCommand.Publish(Osc(OsCommand.SetClipboard, "", param));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_LineFeed()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                Assert.Equal("\n".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\n*1", ws.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.LineFeed, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_NextLine()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.Whitespace = new Event<WhitespaceEvent>("Whitespace", ws =>
            {
                Assert.Equal("\r\n".ToCharArray(), ws.Characters.ToArray());
                Assert.Equal(1, ws.Count);
                Assert.Equal("\r\n*1", ws.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.NextLine, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_ReverseIndex()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.MoveCursor = new Event<MoveCursorEvent>("MoveCursor", move =>
            {
                Assert.Equal(MoveOrigin.Inverse, move.Origin);
                Assert.Equal(MoveAxis.Row, move.Axis);
                Assert.Equal(1, move.Count);
                Assert.Equal("Inverse Row 1", move.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.ReverseIndex, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_IdentifyTerminal()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.IdentifyTerminal = new Event<IdentifyTerminalEvent>("IdentifyTerminal", id =>
            {
                Assert.Equal(string.Empty, id.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.IdentifyTerminal, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_ResetState()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.State = new Event<StateEvent>("State", state =>
            {
                Assert.Equal(States.All, state.States);
                Assert.Equal(StateMode.Reset, state.Mode);
                Assert.Equal("Reset All", state.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.ResetState, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_SaveCursorPosition()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var expected = new HashSet<MoveAxis>
            {
                MoveAxis.Row, MoveAxis.Column
            };
            var dispatched = 0;
            sut.MoveCursor = new Event<MoveCursorEvent>("MoveCursor", move =>
            {
                Assert.Contains(move.Axis, expected);
                expected.Remove(move.Axis);
                Assert.Equal(MoveOrigin.Store, move.Origin);
                Assert.Equal(0, move.Count);
                Assert.Equal(FormattableString.Invariant($"Store {move.Axis} 0"), move.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.SaveCursorPosition, default, IgnoredData.None));
            Assert.Equal(2, dispatched);
            Assert.Empty(expected);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_RestoreCursorPosition()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var expected = new HashSet<MoveAxis>
            {
                MoveAxis.Row, MoveAxis.Column
            };
            var dispatched = 0;
            sut.MoveCursor = new Event<MoveCursorEvent>("MoveCursor", move =>
            {
                Assert.Contains(move.Axis, expected);
                expected.Remove(move.Axis);
                Assert.Equal(MoveOrigin.Restore, move.Origin);
                Assert.Equal(0, move.Count);
                Assert.Equal(FormattableString.Invariant($"Restore {move.Axis} 0"), move.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.RestoreCursorPosition, default, IgnoredData.None));
            Assert.Equal(2, dispatched);
            Assert.Empty(expected);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_SetKeypadApplicationMode()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.State = new Event<StateEvent>("State", state =>
            {
                Assert.Equal(States.KeypadApplicationMode, state.States);
                Assert.Equal(StateMode.Set, state.Mode);
                Assert.Equal("Set KeypadApplicationMode", state.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.SetKeypadApplicationMode, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
        
        [Fact]
        internal static void TerminalEmulator_EscapeSequence_UnsetKeypadApplicationMode()
        {
            var sut = new TerminalEmulator();
            var onEscapeSequence = new Event<VT.Events.EscapeSequenceEvent>("EscapeSequence");
            sut.OnEscapeSequence = onEscapeSequence;

            var dispatched = 0;
            sut.State = new Event<StateEvent>("State", state =>
            {
                Assert.Equal(States.KeypadApplicationMode, state.States);
                Assert.Equal(StateMode.Unset, state.Mode);
                Assert.Equal("Unset KeypadApplicationMode", state.ToString());
                dispatched++;
            });

            onEscapeSequence.Publish(new VT.Events.EscapeSequenceEvent(EscapeCommand.UnsetKeypadApplicationMode, default, IgnoredData.None));
            Assert.Equal(1, dispatched);
        }
    }
}
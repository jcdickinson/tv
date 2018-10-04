using System;
using System.Text;
using Xunit;

namespace TerminalVelocity.Emulator
{
    public static class EmulatorTests
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

            onEsc.Publish(new VT.Events.EscapeSequenceEvent(new byte[] { (byte)'(' }, VT.IgnoredData.None, (byte)'0'));
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
            onEsc.Publish(new VT.Events.EscapeSequenceEvent(new byte[] { (byte)'(' }, VT.IgnoredData.None, (byte)'B'));
            onExec.Publish(new VT.Events.ExecuteEvent(VT.ControlCode.ShiftIn));
            sb.Clear();
            ix = 0;
            current = Case;
            
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            onPrint.Publish(new VT.Events.PrintEvent(Case.AsMemory()));
            Assert.Equal(current + current, sb.ToString());
        }
    }
}
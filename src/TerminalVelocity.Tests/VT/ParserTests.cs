using System;
using System.Text;
using TerminalVelocity.Eventing;
using TerminalVelocity.Pty.Events;
using Xunit;

namespace TerminalVelocity.VT
{
    public static class ParserTests
    {
        private static void DefaultHandler<T>(T e) where T : struct { }

        [Fact]
        public static void Parser_CSI_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#p");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                controlSequenceEvent: new Events.ControlSequenceEvent(csi =>
                {
                    ++dispatched;
                    Assert.Equal('p', csi.Character);
                    Assert.Equal(IgnoredData.None, csi.Ignored);
                    Assert.Equal(new byte[] { 0x21, 0x23 }, csi.Intermediates.ToArray());
                    Assert.Equal(new long[] { 0x01, 0x01, 0x1, 0x1 }, csi.Parameters.ToArray());
                    Assert.Equal("p(01;01;01;01)[!#]", csi.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_CSI_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[p");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                controlSequenceEvent: new Events.ControlSequenceEvent(csi =>
                {
                    ++dispatched;
                    Assert.Equal('p', csi.Character);
                    Assert.Equal(IgnoredData.None, csi.Ignored);
                    Assert.Equal(0, csi.Intermediates.Length);
                    Assert.Equal(0, csi.Parameters.Length);
                    Assert.Equal("p()[]", csi.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_CSI_MaxParams_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#p");

            var options = new ParserOptions
            {
                MaxIntermediates = 1,
                MaxParameters = 2
            };

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(options, onReceiveEvent: receive,
                controlSequenceEvent: new Events.ControlSequenceEvent(csi =>
                {
                    ++dispatched;
                    Assert.Equal('p', csi.Character);
                    Assert.Equal(IgnoredData.All, csi.Ignored);
                    Assert.Equal(new byte[] { 0x21 }, csi.Intermediates.ToArray());
                    Assert.Equal(new long[] { 0x01, 0x01 }, csi.Parameters.ToArray());
                    Assert.Equal("p(01;01...)[!...]", csi.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_CSI_SemiUnderline()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[;4m");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                controlSequenceEvent: new Events.ControlSequenceEvent(csi =>
                {
                    ++dispatched;
                    Assert.Equal('m', csi.Character);
                    Assert.Equal(IgnoredData.None, csi.Ignored);
                    Assert.Equal(new byte[] { }, csi.Intermediates.ToArray());
                    Assert.Equal(new long[] { 0x00, 0x04 }, csi.Parameters.ToArray());
                    Assert.Equal("m(00;04)[]", csi.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_CSI_LongParam()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[9223372036854775808m");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                controlSequenceEvent: new Events.ControlSequenceEvent(csi =>
                {
                    ++dispatched;
                    Assert.Equal('m', csi.Character);
                    Assert.Equal(IgnoredData.None, csi.Ignored);
                    Assert.Equal(new byte[] { }, csi.Intermediates.ToArray());
                    Assert.Equal(new long[] { long.MaxValue }, csi.Parameters.ToArray());
                    Assert.Equal("m(7fffffffffffffff)[]", csi.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_DCS_Hook_Put_Unhook()
        {
            const string PutExpected = " !";
            var packet = Encoding.ASCII.GetBytes("\x1BP1;2 !@ !X");
            packet[packet.Length - 1] = 0x9C;

            var hookDispatched = 0;
            var putDispatched = 0;
            var unhookDispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                hookEvent: new Events.HookEvent(hook =>
                {
                    ++hookDispatched;
                    Assert.Equal(IgnoredData.None, hook.Ignored);
                    Assert.Equal(new long[] { 0x01, 0x02 }, hook.Parameters.ToArray());
                    Assert.Equal(new byte[] { 0x20, 0x21 }, hook.Intermediates.ToArray());
                    Assert.Equal("(01;02)[ !]", hook.ToString());
                }),
                putEvent: new Events.PutEvent(put =>
                {
                    var i = (putDispatched++) % PutExpected.Length;
                    Assert.Equal((byte)PutExpected[i], put.Byte);
                    Assert.Equal(PutExpected[i].ToString(), put.ToString());
                }),
                unhookEvent: new Events.UnhookEvent(unhook =>
                {
                    unhookDispatched++;
                    Assert.Equal(string.Empty, unhook.ToString());
                })
            );

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }

        [Fact]
        public static void Parser_DCS_Hook_Put_Unhook_MaxParams_MaxIntermediates()
        {
            const string PutExpected = " !";

            var packet = Encoding.ASCII.GetBytes("\x1BP1;2;3 !@ !X");
            packet[packet.Length - 1] = 0x9C;

            var options = new ParserOptions()
            {
                MaxIntermediates = 1,
                MaxParameters = 2
            };

            var hookDispatched = 0;
            var putDispatched = 0;
            var unhookDispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(options, onReceiveEvent: receive,
                hookEvent: new Events.HookEvent(hook =>
                {
                    ++hookDispatched;
                    Assert.Equal(IgnoredData.All, hook.Ignored);
                    Assert.Equal(new long[] { 0x01, 0x02 }, hook.Parameters.ToArray());
                    Assert.Equal(new byte[] { 0x20 }, hook.Intermediates.ToArray());
                    Assert.Equal("(01;02...)[ ...]", hook.ToString());
                }),
                putEvent: new Events.PutEvent(put =>
                {
                    var i = (putDispatched++) % PutExpected.Length;
                    Assert.Equal((byte)PutExpected[i], put.Byte);
                    Assert.Equal(PutExpected[i].ToString(), put.ToString());
                }),
                unhookEvent: new Events.UnhookEvent(unhook =>
                {
                    unhookDispatched++;
                    Assert.Equal(string.Empty, unhook.ToString());
                })
            );

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }

        [Fact]
        public static void Parser_ESC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                escapeSequenceEvent: new Events.EscapeSequenceEvent(esc =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, esc.Command);
                    Assert.Equal(IgnoredData.None, esc.Ignored);
                    Assert.Equal(new byte[] { 0x20, 0x21 }, esc.Intermediates.ToArray());
                    Assert.Equal("ConfigureSpecialCharSet[ !]", esc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_ESC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x001B0");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                escapeSequenceEvent: new Events.EscapeSequenceEvent(esc =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, esc.Command);
                    Assert.Equal(IgnoredData.None, esc.Ignored);
                    Assert.Equal(new byte[] { }, esc.Intermediates.ToArray());
                    Assert.Equal("ConfigureSpecialCharSet[]", esc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_ESC_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var options = new ParserOptions()
            {
                MaxIntermediates = 1,
                MaxParameters = 2
            };

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(options, onReceiveEvent: receive,
                escapeSequenceEvent: new Events.EscapeSequenceEvent(esc =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, esc.Command);
                    Assert.Equal(IgnoredData.Intermediates, esc.Ignored);
                    Assert.Equal(new byte[] { 0x20 }, esc.Intermediates.ToArray());
                    Assert.Equal("ConfigureSpecialCharSet[ ...]", esc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_Execute()
        {
            var packet = Encoding.ASCII.GetBytes("\x1C");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                executeEvent: new Events.ExecuteEvent(exec =>
                {
                    ++dispatched;
                    Assert.Equal(ControlCode.FileSeparator, exec.ControlCode);
                    Assert.Equal("FileSeparator", exec.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_OSC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]2;jwilm@jwilm-desk: ~/code/alacritty\x07");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                osCommandEvent: new Events.OsCommandEvent(osc =>
                {
                    ++dispatched;
                    Assert.Equal(1, osc.Length);
                    Assert.Equal(OsCommand.SetWindowTitle, osc.Command);
                    Assert.Equal(osc[0].ToArray(), packet.AsMemory(4, packet.Length - 5).ToArray());

                    Assert.Equal("SetWindowTitle(jwilm@jwilm-desk: ~/code/alacritty)", osc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_OSC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]\x07");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                osCommandEvent: new Events.OsCommandEvent(osc =>
                {
                    ++dispatched;
                    Assert.Equal(1, osc.Length);
                    Assert.Equal(0, osc[0].Length);
                    Assert.Equal(OsCommand.Unknown, osc.Command);
                    Assert.Equal("Unknown()", osc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_OSC_MaxParams()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B];;;;;;;;;;;;;;;;;\x07");

            var options = new ParserOptions
            {
                MaxParameters = 3
            };

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(options, onReceiveEvent: receive,
                osCommandEvent: new Events.OsCommandEvent(osc =>
                {
                    ++dispatched;
                    Assert.Equal(IgnoredData.Parameters, osc.Ignored);
                    Assert.Equal(3, osc.Length);
                    Assert.Equal("Unknown(;;...)", osc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_OSC_UTF8()
        {
            var packet = Encoding.UTF8.GetBytes("\x1B]2;echo '¯\\_(ツ)_/¯' && sleep 1\x07");

            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                osCommandEvent: new Events.OsCommandEvent(osc =>
                {
                    ++dispatched;
                    Assert.Equal(IgnoredData.None, osc.Ignored);
                    Assert.Equal(1, osc.Length);
                    Assert.Equal(OsCommand.SetWindowTitle, osc.Command);
                    Assert.Equal(osc[0].ToArray(), packet.AsMemory(4, packet.Length - 5).ToArray());

                    Assert.Equal("SetWindowTitle(echo '¯\\_(ツ)_/¯' && sleep 1)", osc.ToString());
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void Parser_Print_UTF8()
        {
            const string Case = "Hello 😁 world ";
            var packet = Encoding.UTF8.GetBytes(Case);

            var sb = new StringBuilder();
            var dispatched = 0;
            var receive = new ReceiveEvent(DefaultHandler);
            var parser = new Parser(onReceiveEvent: receive,
                printEvent: new Events.PrintEvent(print =>
                {
                    sb.Append(new string(print.Characters.Span));
                    Assert.Equal(Case.Substring(dispatched, print.Characters.Length), print.ToString());
                    dispatched = (dispatched + print.Characters.Length) % Case.Length;
                }));

            receive.Publish(new ReceiveEventData(packet));
            receive.Publish(new ReceiveEventData(packet));

            Assert.Equal(Case + Case, sb.ToString());
        }
    }
}

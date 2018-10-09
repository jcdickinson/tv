using System;
using System.Text;
using Xunit;

namespace TerminalVelocity.Terminal
{
    public static class VTParserTests
    {
        [Fact]
        public static void VTParser_CSI_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#@");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                ControlSequence = (command, intermediates, parameters, ignored) =>
                {
                    Assert.Equal(ControlSequenceCommand.InsertBlank, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { 0x21, 0x23 }, intermediates.ToArray());
                    Assert.Equal(new long[] { 0x01, 0x01, 0x1, 0x1 }, parameters.ToArray());
                    dispatched++;
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[@");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                ControlSequence = (command, intermediates, parameters, ignored) =>
                {
                    Assert.Equal(ControlSequenceCommand.InsertBlank, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { }, intermediates.ToArray());
                    Assert.Equal(new long[] { }, parameters.ToArray());
                    dispatched++;
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_MaxParams_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#@");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                ControlSequence = (command, intermediates, parameters, ignored) =>
                {
                    Assert.Equal(ControlSequenceCommand.InsertBlank, command);
                    Assert.Equal(IgnoredData.All, ignored);
                    Assert.Equal(new byte[] { 0x21 }, intermediates.ToArray());
                    Assert.Equal(new long[] { 0x01, 0x01 }, parameters.ToArray());
                    dispatched++;
                }
            };
            
            var parser = new VTParser(events: events, maxIntermediates: 1, maxParameters: 2);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_SemiUnderline()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[;4m");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                ControlSequence = (command, intermediates, parameters, ignored) =>
                {
                    Assert.Equal(ControlSequenceCommand.TerminalAttribute, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { }, intermediates.ToArray());
                    Assert.Equal(new long[] { 0x00, 0x04 }, parameters.ToArray());
                    dispatched++;
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_LongParam()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[9223372036854775808m");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                ControlSequence = (command, intermediates, parameters, ignored) =>
                {
                    Assert.Equal(ControlSequenceCommand.TerminalAttribute, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { }, intermediates.ToArray());
                    Assert.Equal(new long[] { long.MaxValue }, parameters.ToArray());
                    dispatched++;
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_DCS_Hook_Put_Unhook()
        {
            const string PutExpected = " !";
            var packet = Encoding.ASCII.GetBytes("\x1BP1;2 !@ !X");
            packet[packet.Length - 1] = 0x9C;

            var hookDispatched = 0;
            var putDispatched = 0;
            var unhookDispatched = 0;
            var events = new VTParserEvents
            {
                Hook = (intermediates, parameters, ignored) =>
                {
                    ++hookDispatched;
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new long[] { 0x01, 0x02 }, parameters.ToArray());
                    Assert.Equal(new byte[] { 0x20, 0x21 }, intermediates.ToArray());
                },
                Put = (byt) =>
                {
                    var i = (putDispatched++) % PutExpected.Length;
                    Assert.Equal((byte)PutExpected[i], byt);
                },
                Unhook = () => unhookDispatched++
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);
            
            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }

        [Fact]
        public static void VTParser_DCS_Hook_Put_Unhook_MaxParams_MaxIntermediates()
        {
            const string PutExpected = " !";

            var packet = Encoding.ASCII.GetBytes("\x1BP1;2;3 !@ !X");
            packet[packet.Length - 1] = 0x9C;


            var hookDispatched = 0;
            var putDispatched = 0;
            var unhookDispatched = 0;
            var events = new VTParserEvents
            {
                Hook = (intermediates, parameters, ignored) =>
                {
                    ++hookDispatched;
                    Assert.Equal(IgnoredData.All, ignored);
                    Assert.Equal(new long[] { 0x01, 0x02 }, parameters.ToArray());
                    Assert.Equal(new byte[] { 0x20 }, intermediates.ToArray());
                },
                Put = (byt) =>
                {
                    var i = (putDispatched++) % PutExpected.Length;
                    Assert.Equal((byte)PutExpected[i], byt);
                },
                Unhook = () => unhookDispatched++
            };

            var parser = new VTParser(events: events, maxIntermediates: 1, maxParameters: 2);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }

        [Fact]
        public static void VTParser_ESC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                EscapeSequence = (command, intermediates, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { 0x20, 0x21 }, intermediates.ToArray());
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_ESC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x001B0");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                EscapeSequence = (command, intermediates, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, command);
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(new byte[] { }, intermediates.ToArray());
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_ESC_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                EscapeSequence = (command, intermediates, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(EscapeCommand.ConfigureSpecialCharSet, command);
                    Assert.Equal(IgnoredData.Intermediates, ignored);
                    Assert.Equal(new byte[] { 0x20 }, intermediates.ToArray());
                }
            };

            var parser = new VTParser(events: events, maxIntermediates: 1);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_Execute()
        {
            var packet = Encoding.ASCII.GetBytes("\x1C");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                Execute = (controlCode) =>
                {
                    ++dispatched;
                    Assert.Equal(ControlCode.FileSeparator, controlCode);
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]2;jwilm@jwilm-desk: ~/code/alacritty\x07");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                OsCommand = (command, parameters, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(1, parameters.Length);
                    Assert.Equal(OsCommand.SetWindowTitle, command);
                    Assert.Equal(parameters[0].ToArray(), packet.AsMemory(4, packet.Length - 5).ToArray());
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]\x07");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                OsCommand = (command, parameters, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(1, parameters.Length);
                    Assert.Equal(0, parameters[0].Length);
                    Assert.Equal(OsCommand.Unknown, command);
                }
            };

            var parser = new VTParser(events: events);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_MaxParams()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B];;;;;;;;;;;;;;;;;\x07");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                OsCommand = (command, parameters, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(IgnoredData.Parameters, ignored);
                    Assert.Equal(3, parameters.Length);
                }
            };

            var parser = new VTParser(events: events, maxParameters: 3);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_UTF8()
        {
            var packet = Encoding.UTF8.GetBytes("\x1B]2;echo '¯\\_(ツ)_/¯' && sleep 1\x07");

            var dispatched = 0;
            var events = new VTParserEvents
            {
                OsCommand = (command, parameters, ignored) =>
                {
                    ++dispatched;
                    Assert.Equal(IgnoredData.None, ignored);
                    Assert.Equal(1, parameters.Length);
                    Assert.Equal(OsCommand.SetWindowTitle, command);
                    Assert.Equal(parameters[0].Span.ToArray(), packet.AsMemory(4, packet.Length - 5).ToArray());
                }
            };

            var parser = new VTParser(events: events, maxParameters: 3);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_Print_UTF8()
        {
            const string Case = "Hello 😁 world ";
            var packet = Encoding.UTF8.GetBytes(Case);
            var dispatched = 0;
            var sb = new StringBuilder();
            var events = new VTParserEvents
            {
                Print = (characters) =>
                {
                    sb.Append(new string(characters));
                    dispatched = (dispatched + characters.Length) % Case.Length;
                }
            };

            var chars = CharacterParser.Create();
            var parser = new VTParser(events: events, maxParameters: 3, utf8: chars.TryParseUtf8, ascii: chars.TryParseAscii);

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(Case + Case, sb.ToString());
        }
    }
}

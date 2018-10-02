using System;
using System.Collections.Generic;
using System.Text;
using SourceCode.Clay.Buffers;
using Xunit;

namespace TerminalVelocity.VT
{
    public static class VTParserTests
    {
        private delegate void VTOscDelegate(in VTOscDispatchAction oscDispatch);
        private delegate void VTPrintDelegate(in VTPrintAction print);
        private delegate void VTCsiDelegate(in VTCsiDispatchAction csiDispath);
        private delegate void VTEscDelegate(in VTEscDispatchAction escDispath);
        private delegate void VTExecuteDelegate(in VTExecuteAction exec);
        private delegate void VTHookDelegate(in VTHookAction hook);
        private delegate void VTPutDelegate(in VTPutAction put);
        private delegate void VTUnhookDelegate(in VTUnhookAction unhook);

        private sealed class EventSinkMock : IVTEventSink
        {
            public VTOscDelegate OscDispatch = (in VTOscDispatchAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTPrintDelegate Print = (in VTPrintAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTCsiDelegate CsiDispatch = (in VTCsiDispatchAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTEscDelegate EscDispatch = (in VTEscDispatchAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTExecuteDelegate Execute = (in VTExecuteAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTHookDelegate Hook = (in VTHookAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTPutDelegate Put = (in VTPutAction _) => Assert.False(true, "Unexpected " + _.ToString());
            public VTUnhookDelegate Unhook = (in VTUnhookAction _) => Assert.False(true, "Unexpected " + _.ToString());

            public void OnCsiDispatch(in VTCsiDispatchAction csiDispatch)
                => CsiDispatch(csiDispatch);

            public void OnEscDispatch(in VTEscDispatchAction escDispatch)
                => EscDispatch(escDispatch);

            public void OnExecute(in VTExecuteAction execute)
                => Execute(execute);

            public void OnHook(in VTHookAction hook)
                => Hook(hook);

            public void OnOscDispatch(in VTOscDispatchAction oscDispatch)
                => OscDispatch(oscDispatch);

            public void OnPrint(in VTPrintAction print)
                => Print(print);

            public void OnPut(in VTPutAction put)
                => Put(put);

            public void OnUnhook(in VTUnhookAction unhook)
                => Unhook(unhook);
        }

        [Fact]
        public static void VTParser_CSI_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#p");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.CsiDispatch = (in VTCsiDispatchAction csi) =>
            {
                ++dispatched;
                Assert.Equal('p', csi.Character);
                Assert.Equal(VTIgnore.None, csi.Ignored);
                BufferAssert.Equal(new byte[] { 0x21, 0x23 }, csi.Intermediates);
                BufferAssert.Equal(new long[] { 0x01, 0x01, 0x1, 0x1 }, csi.Parameters);
                Assert.Equal("CSI Dispatch 70 'p' (01; 01; 01; 01) 21; 23", csi.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[p");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.CsiDispatch = (in VTCsiDispatchAction csi) =>
            {
                ++dispatched;
                Assert.Equal('p', csi.Character);
                Assert.Equal(VTIgnore.None, csi.Ignored);
                Assert.Equal(0, csi.Intermediates.Length);
                Assert.Equal(0, csi.Parameters.Length);
                Assert.Equal("CSI Dispatch 70 'p' ()", csi.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_MaxParams_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[1;1;1;1!#p");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxIntermediates: 1, maxParams: 2);

            var dispatched = 0;
            mock.CsiDispatch = (in VTCsiDispatchAction csi) =>
            {
                ++dispatched;
                Assert.Equal('p', csi.Character);
                Assert.Equal(VTIgnore.All, csi.Ignored);
                BufferAssert.Equal(new byte[] { 0x21 }, csi.Intermediates);
                BufferAssert.Equal(new long[] { 0x01, 0x01 }, csi.Parameters);
                Assert.Equal("CSI Dispatch 70 'p' (01; 01; ignored) 21; ignored", csi.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_SemiUnderline()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[;4m");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.CsiDispatch = (in VTCsiDispatchAction csi) =>
            {
                ++dispatched;
                Assert.Equal('m', csi.Character);
                Assert.Equal(VTIgnore.None, csi.Ignored);
                BufferAssert.Equal(new byte[] { }, csi.Intermediates);
                BufferAssert.Equal(new long[] { 0x00, 0x04 }, csi.Parameters);
                Assert.Equal("CSI Dispatch 6d 'm' (00; 04)", csi.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_CSI_LongParam()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B[9223372036854775808m");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.CsiDispatch = (in VTCsiDispatchAction csi) =>
            {
                ++dispatched;
                Assert.Equal('m', csi.Character);
                Assert.Equal(VTIgnore.None, csi.Ignored);
                BufferAssert.Equal(new byte[] { }, csi.Intermediates);
                BufferAssert.Equal(new long[] { long.MaxValue }, csi.Parameters);
                Assert.Equal("CSI Dispatch 6d 'm' (7fffffffffffffff)", csi.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_ESC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.EscDispatch = (in VTEscDispatchAction esc) =>
            {
                ++dispatched;
                Assert.Equal((byte)'0', esc.Byte);
                Assert.Equal(VTIgnore.None, esc.Ignored);
                BufferAssert.Equal(new byte[] { 0x20, 0x21 }, esc.Intermediates);
                Assert.Equal("ESC Dispatch 30 20; 21", esc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_ESC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x001B0");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.EscDispatch = (in VTEscDispatchAction esc) =>
            {
                ++dispatched;
                Assert.Equal((byte)'0', esc.Byte);
                Assert.Equal(VTIgnore.None, esc.Ignored);
                BufferAssert.Equal(new byte[] { }, esc.Intermediates);
                Assert.Equal("ESC Dispatch 30", esc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_ESC_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B !0");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxIntermediates: 1);

            var dispatched = 0;
            mock.EscDispatch = (in VTEscDispatchAction esc) =>
            {
                ++dispatched;
                Assert.Equal((byte)'0', esc.Byte);
                Assert.Equal(VTIgnore.Intermediates, esc.Ignored);
                BufferAssert.Equal(new byte[] { 0x20 }, esc.Intermediates);
                Assert.Equal("ESC Dispatch 30 20; ignored", esc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_Execute()
        {
            var packet = Encoding.ASCII.GetBytes("\x1C");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.Execute = (in VTExecuteAction exec) =>
            {
                ++dispatched;
                Assert.Equal(VTControlCode.FileSeparator, exec.ControlCode);
                Assert.Equal("Execute FileSeparator", exec.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_Hook_Put_Unhook()
        {
            var packet = Encoding.ASCII.GetBytes("\x1BP1;2 !@ !X");
            packet[packet.Length - 1] = 0x9C;

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var hookDispatched = 0;
            mock.Hook = (in VTHookAction hook) =>
            {
                ++hookDispatched;
                Assert.Equal(VTIgnore.None, hook.Ignored);
                BufferAssert.Equal(new long[] { 0x01, 0x02 }, hook.Parameters);
                BufferAssert.Equal(new byte[] { 0x20, 0x21 }, hook.Intermediates);
                Assert.Equal("Hook (01; 02) 20; 21", hook.ToString());
            };

            var putDispatched = 0;
            var putExpected = new byte[] { 0x20, 0x21 };
            mock.Put = (in VTPutAction put) =>
            {
                var i = (putDispatched++) % putExpected.Length;
                Assert.Equal(putExpected[i], put.Byte);
                Assert.Equal($"Put {putExpected[i]:x2}", put.ToString());
            };

            var unhookDispatched = 0;
            mock.Unhook = (in VTUnhookAction unhook) =>
            {
                unhookDispatched++;
                Assert.Equal("Unhook", unhook.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }
        
        [Fact]
        public static void VTParser_Hook_Put_Unhook_MaxParams_MaxIntermediates()
        {
            var packet = Encoding.ASCII.GetBytes("\x1BP1;2;3 !@ !X");
            packet[packet.Length - 1] = 0x9C;

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxIntermediates: 1, maxParams: 2);

            var hookDispatched = 0;
            mock.Hook = (in VTHookAction hook) =>
            {
                ++hookDispatched;
                Assert.Equal(VTIgnore.All, hook.Ignored);
                BufferAssert.Equal(new long[] { 0x01, 0x02 }, hook.Parameters);
                BufferAssert.Equal(new byte[] { 0x20 }, hook.Intermediates);
                Assert.Equal("Hook (01; 02; ignored) 20; ignored", hook.ToString());
            };

            var putDispatched = 0;
            var putExpected = new byte[] { 0x20, 0x21 };
            mock.Put = (in VTPutAction put) =>
            {
                var i = (putDispatched++) % putExpected.Length;
                Assert.Equal(putExpected[i], put.Byte);
                Assert.Equal($"Put {putExpected[i]:x2}", put.ToString());
            };

            var unhookDispatched = 0;
            mock.Unhook = (in VTUnhookAction unhook) =>
            {
                unhookDispatched++;
                Assert.Equal("Unhook", unhook.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, hookDispatched);
            Assert.Equal(4, putDispatched);
            Assert.Equal(2, unhookDispatched);
        }

        [Fact]
        public static void VTParser_OSC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]2;jwilm@jwilm-desk: ~/code/alacritty\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(2, osc.Length);
                BufferAssert.Equal(osc[0], packet.AsSpan(2, 1));
                BufferAssert.Equal(osc[1], packet.AsSpan(4, packet.Length - 5));
                
                Assert.Equal("OSC Dispatch 32; 6a, 77, 69, 6c, 6d, 40, 6a, 77, 69, 6c, 6d, 2d, 64, 65, 73, 6b, 3a, 20, 7e, 2f, 63, 6f, 64, 65, 2f, 61, 6c, 61, 63, 72, 69, 74, 74, 79", osc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(1, osc.Length);
                Assert.Equal(0, osc[0].Length);

                Assert.Equal("OSC Dispatch ", osc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_MaxParams()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B];;;;;;;;;;;;;;;;;\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxParams: 3);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(VTIgnore.Parameters, osc.Ignored);
                Assert.Equal(3, osc.Length);
                Assert.Equal("OSC Dispatch ; ; ; ignored", osc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_OSC_UTF8()
        {
            var packet = Encoding.UTF8.GetBytes("\x1B]2;echo '¯\\_(ツ)_/¯' && sleep 1\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxParams: 3);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(VTIgnore.None, osc.Ignored);
                Assert.Equal(2, osc.Length);
                BufferAssert.Equal(osc[0], packet.AsSpan(2, 1));
                BufferAssert.Equal(osc[1], packet.AsSpan(4, packet.Length - 5));

                Assert.Equal("OSC Dispatch 32; 65, 63, 68, 6f, 20, 27, c2, af, 5c, 5f, 28, e3, 83, 84, 29, 5f, 2f, c2, af, 27, 20, 26, 26, 20, 73, 6c, 65, 65, 70, 20, 31", osc.ToString());
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public static void VTParser_Print_Utf8()
        {
            const string Case = "Hello 😁 world ";
            var packet = Encoding.UTF8.GetBytes(Case);

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxParams: 3);

            var sb = new StringBuilder();
            var ix = 0;
            mock.Print = (in VTPrintAction print) =>
            {
                sb.Append(new string(print.Characters));

                Assert.Equal("Print " + Case.Substring(ix, print.Characters.Length), print.ToString());
                ix = (ix + print.Characters.Length) % Case.Length;
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(Case + Case, sb.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SourceCode.Clay.Buffers;
using Xunit;

namespace TerminalVelocity.VT
{
    public class VTParserTests
    {
        private delegate void VTOscDelegate(in VTOscDispatchAction oscDispatch);
        private delegate void VTPrintDelegate(in VTPrintAction print);

        private class EventSinkMock : IVTEventSink
        {
            public VTOscDelegate OscDispatch;
            public VTPrintDelegate Print;

            public void OnCsiDispatch(in VTCsiDispatchAction csiDispatch)
            {
                throw new NotImplementedException();
            }

            public void OnEscDispatch(in VTEscDispatchAction escDispatch)
            {
                throw new NotImplementedException();
            }

            public void OnExecute(in VTExecuteAction execute)
            {
                throw new NotImplementedException();
            }

            public void OnHook(in VTHookAction hook)
            {
                throw new NotImplementedException();
            }

            public void OnOscDispatch(in VTOscDispatchAction oscDispatch)
                => OscDispatch(oscDispatch);

            public void OnPrint(in VTPrintAction print)
                => Print(print);

            public void OnPut(in VTPutAction put)
            {
                throw new NotImplementedException();
            }

            public void OnUnhook(in VTUnhookAction unhook)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void VTParser_Print_Utf8()
        {
            const string Case = "Hello 😁 world ";
            var packet = Encoding.UTF8.GetBytes(Case);

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxParams: 3);

            var sb = new StringBuilder();
            mock.Print = (in VTPrintAction print) =>
            {
                sb.Append(new string(print.Characters));
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(Case + Case, sb.ToString());
        }

        [Fact]
        public void VTParser_OSC_Populated()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]2;jwilm@jwilm-desk: ~/code/alacritty\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(2, osc.Parameters.Length);
                Assert.Equal(osc.Parameters[0], packet.AsMemory(2, 1), BufferComparer.Memory);
                Assert.Equal(osc.Parameters[1], packet.AsMemory(4, packet.Length - 5), BufferComparer.Memory);
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public void VTParser_OSC_Empty()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B]\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(1, osc.Parameters.Length);
                Assert.Equal(0, osc.Parameters[0].Length);
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }

        [Fact]
        public void VTParser_OSC_MaxParams()
        {
            var packet = Encoding.ASCII.GetBytes("\x1B];;;;;;;;;;;;;;;;;\x07");

            var mock = new EventSinkMock();
            var parser = new VTParser(mock, maxParams: 3);

            var dispatched = 0;
            mock.OscDispatch = (in VTOscDispatchAction osc) =>
            {
                ++dispatched;
                Assert.Equal(3, osc.Parameters.Length);
            };

            parser.Process(packet);
            parser.Process(packet);

            Assert.Equal(2, dispatched);
        }
    }
}

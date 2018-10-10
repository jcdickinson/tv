using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Xunit;

namespace TerminalVelocity.WinPty
{
    public class LibTests
    {
        [Fact]
        public void Lib_Load()
        {
            var lib = Lib.Create();
        }

        private byte[] ReadToEnd(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [Fact]
        public void Lib_ExecuteCmd()
        {
            var lib = Lib.Create();
            IntPtr agentConfig = lib.ConfigNew(Lib.AgentOptions.None, out IntPtr error);
            Assert.NotEqual(IntPtr.Zero, agentConfig);
            IntPtr pty = lib.Open(agentConfig, out error);
            lib.Free(agentConfig);
            Assert.NotEqual(IntPtr.Zero, pty);

            try
            {
                var inName = lib.ConInName(pty);
                var outName = lib.ConOutName(pty);
                Lib.ParsePipeName(ref inName, out var inServer);
                Lib.ParsePipeName(ref outName, out var outServer);

                Assert.NotNull(inName);
                Assert.NotNull(outName);

                using (var inPipe = new NamedPipeClientStream(inServer, inName, PipeDirection.Out))
                using (var outPipe = new NamedPipeClientStream(outServer, outName, PipeDirection.In))
                {
                    inPipe.Connect(1000);
                    outPipe.Connect(1000);

                    IntPtr spawnConfig = lib.SpawnConfigNew(
                        Lib.SpawnOptions.AutoShutdown,
                        "C:\\windows\\system32\\cmd.exe",
                        "",
                        null, null, out error);
                    Assert.NotEqual(IntPtr.Zero, spawnConfig);

                    var spawned = lib.Spawn(
                        pty,
                        spawnConfig,
                        out IntPtr processHandle,
                        out IntPtr threadHandle,
                        out var createProcessError,
                        out error);
                    lib.SpawnConfigFree(spawnConfig);

                    Assert.True(spawned);


                    var g = Guid.NewGuid().ToString("N");
                    var input = $"prompt $g\r\nrem {g}\r\nexit\r\n";
                    inPipe.Write(Encoding.ASCII.GetBytes(input));

                    var result = Encoding.ASCII.GetString(ReadToEnd(outPipe));
                    Assert.Contains(g, result);
                }
            }
            finally
            {
                lib.Free(pty);
            }
        }
    }
}

/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
using TerminalVelocity.Eventing;
using TerminalVelocity.Plugins;
using TerminalVelocity.Terminal;
using TerminalVelocity.Terminal.Events;

namespace TerminalVelocity.WinPty
{
    public class WinPtyPlugin : IPlugin
    {
        private struct Pty
        {
            public readonly IntPtr Handle;
            public readonly NamedPipeClientStream In;
            public readonly NamedPipeClientStream Out;
            public readonly NamedPipeClientStream Error;

            public Pty(
                IntPtr handle,
                NamedPipeClientStream @in,
                NamedPipeClientStream @out,
                NamedPipeClientStream error)
            {
                Handle = handle;
                In = @in;
                Out = @out;
                Error = error;
            }
        }

        private readonly ConcurrentDictionary<TerminalIdentifier, Pty> _terminals;
        private readonly Lib _lib;

        private readonly ConsoleOutEvent _consoleOutEvent;
        private readonly ConsoleErrorEvent _consoleErrorEvent;
        private readonly TerminalOpenedEvent _terminalOpened;
        private readonly TerminalClosedEvent _terminalClosed;

        public WinPtyPlugin(
            ConsoleOutEvent consoleOutEvent = null,
            ConsoleErrorEvent consoleErrorEvent = null,
            TerminalOpenedEvent terminalOpened = null,
            TerminalClosedEvent terminalClosed = null,
            TerminalOpenEvent onTerminalOpen = null,
            TerminalCloseEvent onTerminalClose = null,
            ConsoleInEvent onConsoleInEvent = null)
        {
            _lib = Lib.Create();
            _terminals = new ConcurrentDictionary<TerminalIdentifier, Pty>();

            _consoleOutEvent = consoleOutEvent;
            _consoleErrorEvent = consoleErrorEvent;
            _terminalOpened = terminalOpened;
            _terminalClosed = terminalClosed;

            onTerminalOpen?.Subscribe(OnTerminalOpen);
            onTerminalClose?.Subscribe(OnTerminalClose);
            onConsoleInEvent?.Subscribe(OnConsoleIn);
        }

        private EventStatus OnTerminalOpen(in TerminalOpenEventData e)
        {
            var free = new IntPtr[3];
            var dispose = new NamedPipeClientStream[3];
            try
            {
                TerminalOpenEventData d = e;
                Pty pty = _terminals.GetOrAdd(e.Terminal, id =>
                 {
                     free[0] = _lib.CheckResult(
                         _lib.ConfigNew(Lib.AgentOptions.ColorEscapes | Lib.AgentOptions.ConError, out IntPtr error),
                         error, "Failed to create WinPTY configuration.");

                     free[1] = _lib.CheckResult(
                         _lib.Open(free[0], out error),
                         error, "Failed to create WinPTY instance.");

                     var inName = _lib.ConInName(free[1]);
                     var errorName = _lib.ConErrorName(free[1]);
                     var outName = _lib.ConOutName(free[1]);

                     if (string.IsNullOrEmpty(inName)) throw new InvalidOperationException("Failed to acquire WinPTY input pipe.");
                     if (string.IsNullOrEmpty(errorName)) throw new InvalidOperationException("Failed to acquire WinPTY error pipe.");
                     if (string.IsNullOrEmpty(outName)) throw new InvalidOperationException("Failed to acquire WinPTY output pipe.");

                     Lib.ParsePipeName(ref inName, out var inServer);
                     Lib.ParsePipeName(ref errorName, out var errorServer);
                     Lib.ParsePipeName(ref outName, out var outServer);

                     dispose[0] = new NamedPipeClientStream(inServer, inName, PipeDirection.Out);
                     dispose[1] = new NamedPipeClientStream(errorServer, errorName, PipeDirection.In);
                     dispose[2] = new NamedPipeClientStream(outServer, outName, PipeDirection.In);

                     dispose[0].Connect(1000);
                     dispose[1].Connect(1000);
                     dispose[2].Connect(1000);

                     free[2] = _lib.CheckResult(
                         _lib.SpawnConfigNew(Lib.SpawnOptions.AutoShutdown, d.ApplicationName, d.Arguments, d.WorkingDirectory, d.Environment, out error),
                         error, "Failed to create WinPTY spawn config.");

                     _lib.CheckResult(
                         _lib.Spawn(free[1], free[2], out IntPtr processHandle, out IntPtr threadHandle, out var createProcessHandle, out error),
                         error, "Failed to spawn WinPTY process.");

                     return new Pty(free[1], dispose[0], dispose[2], dispose[1]);
                 });

                if (pty.Handle == free[1])
                {
                    free[1] = IntPtr.Zero;
                    dispose[0] = dispose[1] = dispose[2] = null;
                    Worker(pty.Out, e.Terminal, _consoleOutEvent, (t, m) => new ConsoleOutEventData(t, m));
                    Worker(pty.Error, e.Terminal, _consoleErrorEvent, (t, m) => new ConsoleErrorEventData(t, m));
                    _terminalOpened?.Publish(new TerminalOpenedEventData(e.Terminal));
                }
                else
                {
                    throw new InvalidOperationException("Terminal already created.");
                }

                return EventStatus.Halt;
            }
            catch
            {
                // TODO: Logging
                return EventStatus.Continue;
            }
            finally
            {
                using (dispose[0])
                using (dispose[1])
                using (dispose[2])
                { }

                var i = 0;
                if (free[i] != IntPtr.Zero) _lib.ConfigFree(free[i++]);
                if (free[i] != IntPtr.Zero) _lib.Free(free[i++]);
                if (free[i] != IntPtr.Zero) _lib.SpawnConfigFree(free[i++]);
            }
        }

        private async void Worker<TEventData>(
            NamedPipeClientStream stream,
            TerminalIdentifier terminal,
            Event<TerminalEventLoop, TEventData> evt,
            Func<TerminalIdentifier, ReadOnlyMemory<byte>, TEventData> dataFactory)
            where TEventData : struct
        {
            try
            {
                while (true)
                {
                    var buffer = new byte[100];
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None).ConfigureAwait(false);
                    if (read == 0) break;
                    evt?.Publish(dataFactory(terminal, buffer.AsMemory(0, read)));
                }
            }
            catch { }
            finally
            {
                OnTerminalClose(new TerminalCloseEventData(terminal));
            }
        }

        private EventStatus OnTerminalClose(in TerminalCloseEventData e)
        {
            if (_terminals.TryRemove(e.Terminal, out Pty pty))
            {
                Dispose(pty);
                _terminalClosed?.Publish(new TerminalClosedEventData(e.Terminal));
                return EventStatus.Halt;
            }
            return EventStatus.Continue;
        }

        private EventStatus OnConsoleIn(in ConsoleInEventData e)
        {
            if (_terminals.TryGetValue(e.Terminal, out Pty pty))
            {
                try
                {
                    pty.In.Write(e.Buffer.Span);
                }
                catch
                {
                    OnTerminalClose(new TerminalCloseEventData(e.Terminal));
                }
                return EventStatus.Halt;
            }
            return EventStatus.Continue;
        }

        public void Dispose()
        {
            foreach (Pty value in _terminals.Values)
                Dispose(value);
            _terminals.Clear();
        }

        private void Dispose(Pty pty)
        {
            using (pty.In)
            using (pty.Out)
            using (pty.Error)
            {
                _lib.Free(pty.Handle);
            }
        }
    }
}

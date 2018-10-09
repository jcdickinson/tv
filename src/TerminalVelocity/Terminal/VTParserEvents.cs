using System;

namespace TerminalVelocity.Terminal
{
    internal struct VTParserEvents
    {
        public delegate void PrintEvent(ReadOnlySpan<char> characters);
        public delegate void ExecuteEvent(ControlCode controlCode);
        public delegate void HookEvent(ReadOnlySpan<byte> intermediates, ReadOnlySpan<long> parameters, IgnoredData ignored);
        public delegate void PutEvent(byte @byte);
        public delegate void OsCommandEvent(OsCommand command, ReadOnlySpan<ReadOnlyMemory<byte>> parameters, IgnoredData ignored);
        public delegate void UnhookEvent();
        public delegate void ControlSequenceEvent(ControlSequenceCommand command, ReadOnlySpan<byte> intermediates, ReadOnlySpan<long> parameters, IgnoredData ignored);
        public delegate void EscapeSequenceEvent(EscapeCommand command, ReadOnlySpan<byte> intermediates, IgnoredData ignored);

        public PrintEvent Print;
        public ExecuteEvent Execute;
        public HookEvent Hook;
        public PutEvent Put;
        public OsCommandEvent OsCommand;
        public UnhookEvent Unhook;
        public ControlSequenceEvent ControlSequence;
        public EscapeSequenceEvent EscapeSequence;
    }
}

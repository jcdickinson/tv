using System;
using System.Composition;
using TerminalVelocity.Pty.Events;
using TerminalVelocity.VT.Events;

namespace TerminalVelocity.VT
{
    // https://github.com/jwilm/vte/blob/master/src/lib.rs

    [Shared]
    public sealed class Parser
    {
        [Flags]
        private enum ParseOptions : byte
        {
            None = 0,
            CollectingParameters = 0b0000_0100,
            IgnoringIntermediates = 0b0000_00001,
            IgnoringParameters = 0b0000_00010,
            Ignoring = IgnoringIntermediates | IgnoringParameters
        }
        
        [Import(ControlSequenceEvent.ContractName)]
        public Event<ControlSequenceEvent> ControlSequence { private get; set; }
        
        [Import(EscapeSequenceEvent.ContractName)]
        public Event<EscapeSequenceEvent> EscapeSequence { private get; set; }
        
        [Import(ExecuteEvent.ContractName)]
        public Event<ExecuteEvent> Execute { private get; set; }

        [Import(HookEvent.ContractName)]
        public Event<HookEvent> Hook { private get; set; }

        [Import(OsCommandEvent.ContractName)]
        public Event<OsCommandEvent> OsCommand { private get; set; }

        [Import(PrintEvent.ContractName)]
        public Event<PrintEvent> Print { private get; set; }

        [Import(PutEvent.ContractName)]
        public Event<PutEvent> Put { private get; set; }
        
        [Import(UnhookEvent.ContractName)]
        public Event<UnhookEvent> Unhook { private get; set; }

        private ParserState _state;
        private readonly byte[] _intermediates;
        private readonly long[] _params;
        private readonly ReadOnlyMemory<byte>[] _oscParams;
        private readonly byte[] _oscRaw;
        private readonly ParserUtf8 _utf8;

        private ParseOptions _options;
        private int _intermediateIndex;
        private long _param;
        private int _numParams;
        private int _oscIndex;
        private int _previousOscEnd;
        private int _oscNumParams;

        public Parser(
            int maxIntermediates = 2,
            int maxOscRaw = 1024,
            int maxParams = 16)
        {
            if (maxIntermediates < 0) throw new ArgumentOutOfRangeException(nameof(maxIntermediates));
            if (maxOscRaw < 0) throw new ArgumentOutOfRangeException(nameof(maxOscRaw));
            if (maxParams < 0) throw new ArgumentOutOfRangeException(nameof(maxParams));

            _state = ParserState.Ground;
            _intermediates = new byte[maxIntermediates];
            _params = new long[maxParams];
            _oscRaw = new byte[maxOscRaw];
            _oscParams = new ReadOnlyMemory<byte>[maxParams];

            _utf8 = new ParserUtf8(true);
        }

        [Import(ReceiveEvent.ContractName)]
        public Event<ReceiveEvent> OnReceive
        {
            set => value.Subscribe((ref ReceiveEvent receive) =>
            {
                var next = receive.Data.Span;
                for (var i = 0; i < next.Length; i++)
                    Process(next[i]);
            });
        }

        private void Process(byte next)
        {
            if (_state == ParserState.Utf8)
            {
                ProcessUtf8(next);
                return;
            }

            // Handle state changes in the anywhere state before evaluating changes
            // for current state.
            var change = ParserState.Anywhere.GetStateChange(next);
            if (change.IsEmpty)
                change = _state.GetStateChange(next);

            PerformChange(change, next);
        }

        private void ProcessUtf8(byte next)
        {
            if (!_utf8.Process(next, out var result))
                return;

            Print.Publish(new PrintEvent(result));
            _state = ParserState.Ground;
        }

        private void PerformChange(ParserStateAction change, byte next)
        {
            if (change.State == ParserState.Anywhere)
            {
                PerformAction(change, next);
                return;
            }

            PerformAction(new ParserStateAction(_state).WithExitAction(), 0);
            PerformAction(change, next);
            PerformAction(change.WithEntryAction(), 0);
            if (change.State != ParserState.Anywhere)
                _state = change.State;
        }

        private void PerformAction(ParserStateAction change, byte next)
        {
            if (change.Action == ParserAction.None)
                return;
                
            var (_, action) = change;

            switch (action)
            {
                case ParserAction.Print:
                    Print.Publish(new PrintEvent(_utf8.Provide(next)));
                    return;
                case ParserAction.Execute:
                    Execute.Publish(new ExecuteEvent((ControlCode)next));
                    return;
                case ParserAction.Hook:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;
                        
                    Hook.Publish(new HookEvent(
                        _params.AsMemory(0, _numParams), 
                        _intermediates.AsMemory(0, _intermediateIndex), 
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring)));
                        
                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case ParserAction.Put:
                    Put.Publish(new PutEvent(next));
                    return;
                case ParserAction.OscStart:
                    _oscIndex = 0;
                    _oscNumParams = 0;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case ParserAction.OscPut:
                    if (_oscIndex == _oscRaw.Length)
                        return;

                    // Param separator
                    if (next == ';')
                    {
                        if (_oscNumParams == _oscParams.Length)
                        {
                            _options |= ParseOptions.IgnoringParameters;
                            return;
                        }
                        // First param is special - 0 to current byte index
                        else if (_oscNumParams == 0)
                        {
                            _oscParams[_oscNumParams++] = _oscRaw.AsMemory(0, _oscIndex);
                            _previousOscEnd = _oscIndex;
                            return;
                        }
                        // All other params depend on previous indexing
                        else
                        {
                            _oscParams[_oscNumParams++] = _oscRaw.AsMemory(_previousOscEnd, _oscIndex - _previousOscEnd);
                            _previousOscEnd = _oscIndex;
                            return;
                        }
                    }

                    _oscRaw[_oscIndex++] = next;
                    return;
                case ParserAction.OscEnd:
                    if (_oscIndex == _oscRaw.Length)
                        return;

                    if (_oscNumParams == _oscParams.Length)
                        _options |= ParseOptions.IgnoringParameters;
                    // First param is special - 0 to current byte index
                    else if (_oscNumParams == 0)
                        _oscParams[_oscNumParams++] = _oscRaw.AsMemory(0, _oscIndex);
                    // All other params depend on previous indexing
                    else
                        _oscParams[_oscNumParams++] = _oscRaw.AsMemory(_previousOscEnd, _oscIndex - _previousOscEnd);

                    OsCommand.Publish(new OsCommandEvent(
                        _oscParams.AsMemory(0, _oscNumParams),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring)));
                    return;
                case ParserAction.Unhook:
                    Unhook.Publish(default);
                    return;
                case ParserAction.CsiDispatch:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;

                    ControlSequence.Publish(new ControlSequenceEvent(
                        _intermediates.AsMemory(0, _intermediateIndex),
                        _params.AsMemory(0, _numParams),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring),
                        (char)next
                    ));
                    
                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case ParserAction.EscDispatch:
                    EscapeSequence.Publish(new EscapeSequenceEvent(
                        _intermediates.AsMemory(0, _intermediateIndex),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring),
                        next
                    ));
                    return;
                case ParserAction.Ignore:
                case ParserAction.None:
                    return;
                case ParserAction.Collect:
                    if (_intermediateIndex == _intermediates.Length)
                        _options |= ParseOptions.IgnoringIntermediates;
                    else
                        _intermediates[_intermediateIndex++] = next;
                    return;
                case ParserAction.Param:
                    if (next == ';')
                    {
                        if (_numParams == _params.Length - 1)
                        {
                            _options |= ParseOptions.IgnoringParameters;
                            return;
                        }
                        _params[_numParams++] = _param;
                        _param = 0;
                        _options &= ~ParseOptions.CollectingParameters;
                        return;
                    }
                    else if (!_options.HasFlag(ParseOptions.IgnoringParameters))
                    {
                        MulAdd(ref _param, next - '0');
                        _options |= ParseOptions.CollectingParameters;
                    }
                    return;
                case ParserAction.Clear:
                    _intermediateIndex = 0;
                    _numParams = 0;
                    _options &= ~ParseOptions.IgnoringIntermediates;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case ParserAction.BeginUtf8:
                    ProcessUtf8(next);
                    return;
            }
        }

        private static void MulAdd(ref long a, int b)
        {
            var na = (a * 10) + b;
            if (a < 0 && na >= 0)
                a = long.MinValue;
            else if (a >= 0 && na < 0)
                a = long.MaxValue;
            else
                a = na;
        }
    }
}
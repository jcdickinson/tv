using System;
using TerminalVelocity.Eventing;
using TerminalVelocity.Pty.Events;
using TerminalVelocity.VT.Events;

namespace TerminalVelocity.VT
{
    // https://github.com/jwilm/vte/blob/master/src/lib.rs

    public sealed class ParserOptions
    {
        private int _maxIntermediates = 2;
        private int _maxOsCommandRaw = 1024;
        private int _maxParameters = 16;

        public int MaxIntermediates
        {
            get => _maxIntermediates;
            set => _maxIntermediates = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public int MaxOsCommandRaw
        {
            get => _maxOsCommandRaw;
            set => _maxOsCommandRaw = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public int MaxParameters
        {
            get => _maxParameters;
            set => _maxParameters = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        public ParserOptions()
        {

        }
    }

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

        private readonly ControlSequenceEvent _controlSequenceEvent;
        private readonly EscapeSequenceEvent _escapeSequenceEvent;
        private readonly ExecuteEvent _executeEvent;
        private readonly HookEvent _hookEvent;
        private readonly OsCommandEvent _osCommandEvent;
        private readonly PrintEvent _printEvent;
        private readonly PutEvent _putEvent;
        private readonly UnhookEvent _unhookEvent;

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
            ParserOptions options = null,
            ReceiveEvent onReceiveEvent = null,

            ControlSequenceEvent controlSequenceEvent = null,
            EscapeSequenceEvent escapeSequenceEvent = null,
            ExecuteEvent executeEvent = null,
            HookEvent hookEvent = null,
            OsCommandEvent osCommandEvent = null,
            PrintEvent printEvent = null,
            PutEvent putEvent = null,
            UnhookEvent unhookEvent = null)
        {
            _controlSequenceEvent = controlSequenceEvent;
            _escapeSequenceEvent = escapeSequenceEvent;
            _executeEvent = executeEvent;
            _hookEvent = hookEvent;
            _osCommandEvent = osCommandEvent;
            _printEvent = printEvent;
            _putEvent = putEvent;
            _unhookEvent = unhookEvent;

            _state = ParserState.Ground;
            _intermediates = new byte[options?.MaxIntermediates ?? 2];
            _params = new long[options?.MaxParameters ?? 16];
            _oscRaw = new byte[options?.MaxOsCommandRaw ?? 1024];
            _oscParams = new ReadOnlyMemory<byte>[options?.MaxParameters ?? 16];

            _utf8 = new ParserUtf8(true);

            onReceiveEvent?.Subscribe(OnReceiveEvent);
        }

        public EventStatus OnReceiveEvent(in ReceiveEventData receive)
        {
            ReadOnlySpan<byte> next = receive.Data.Span;
            for (var i = 0; i < next.Length; i++)
                Process(next[i]);
            return EventStatus.Continue;
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
            ParserStateAction change = ParserState.Anywhere.GetStateChange(next);
            if (change.IsEmpty)
                change = _state.GetStateChange(next);

            PerformChange(change, next);
        }

        private void ProcessUtf8(byte next)
        {
            if (!_utf8.Process(next, out ReadOnlyMemory<char> result))
                return;

            _printEvent?.Publish(new PrintEventData(result));
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

            (ParserState _, ParserAction action) = change;

            switch (action)
            {
                case ParserAction.Print:
                    _printEvent?.Publish(new PrintEventData(_utf8.Provide(next)));
                    return;
                case ParserAction.Execute:
                    _executeEvent?.Publish(new ExecuteEventData((ControlCode)next));
                    return;
                case ParserAction.Hook:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;

                    _hookEvent?.Publish(new HookEventData(
                        _params.AsMemory(0, _numParams),
                        _intermediates.AsMemory(0, _intermediateIndex),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring)));

                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case ParserAction.Put:
                    _putEvent?.Publish(new PutEventData(next));
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
                    else
                        _oscParams[_oscNumParams++] = _oscNumParams == 0
                            ? (ReadOnlyMemory<byte>)_oscRaw.AsMemory(0, _oscIndex)
                            : (ReadOnlyMemory<byte>)_oscRaw.AsMemory(_previousOscEnd, _oscIndex - _previousOscEnd);

                    _osCommandEvent?.Publish(new OsCommandEventData(
                        _oscParams.AsMemory(0, _oscNumParams),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring)));
                    return;
                case ParserAction.Unhook:
                    _unhookEvent?.Publish(default);
                    return;
                case ParserAction.CsiDispatch:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;

                    _controlSequenceEvent?.Publish(new ControlSequenceEventData(
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
                    _escapeSequenceEvent?.Publish(new EscapeSequenceEventData(
                        (EscapeCommand)next,
                        _intermediates.AsMemory(0, _intermediateIndex),
                        (IgnoredData)(byte)(_options & ParseOptions.Ignoring)
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
            a = a < 0 && na >= 0
                ? long.MinValue
                : a >= 0 && na < 0
                    ? long.MaxValue
                    : na;
        }
    }
}

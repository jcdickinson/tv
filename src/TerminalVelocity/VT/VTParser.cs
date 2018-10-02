using System;

namespace TerminalVelocity.VT
{
    // https://github.com/jwilm/vte/blob/master/src/lib.rs

    public sealed class VTParser
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

        private VTParserState _state = VTParserState.Ground;
        private readonly byte[] _intermediates;
        private readonly long[] _params;
        private readonly ReadOnlyMemory<byte>[] _oscParams;
        private readonly byte[] _oscRaw;
        private readonly IVTEventSink _eventSink;

        private ParseOptions _options;
        private int _intermediateIndex;
        private long _param;
        private int _numParams;
        private int _oscIndex;
        private int _previousOscEnd;
        private int _oscNumParams;
        private Utf8 _utf8;

        public VTParser(IVTEventSink eventSink, int maxIntermediates = 2, int maxOscRaw = 1024, int maxParams = 16)
        {
            if (eventSink == null) throw new ArgumentNullException(nameof(eventSink));
            if (maxIntermediates < 0) throw new ArgumentOutOfRangeException(nameof(maxIntermediates));
            if (maxOscRaw < 0) throw new ArgumentOutOfRangeException(nameof(maxOscRaw));
            if (maxParams < 0) throw new ArgumentOutOfRangeException(nameof(maxParams));

            _state = VTParserState.Ground;
            _eventSink = eventSink;
            _intermediates = new byte[maxIntermediates];
            _params = new long[maxParams];
            _oscRaw = new byte[maxOscRaw];
            _oscParams = new ReadOnlyMemory<byte>[maxParams];

            _utf8 = new Utf8(true);
        }

        public void Process(ReadOnlySpan<byte> next)
        {
            for (var i = 0; i < next.Length; i++)
                Process(next[i]);
        }

        public void Process(byte next)
        {
            if (_state == VTParserState.Utf8)
            {
                ProcessUtf8(next);
                return;
            }

            // Handle state changes in the anywhere state before evaluating changes
            // for current state.
            var change = VTParserState.Anywhere.GetStateChange(next);
            if (change.IsEmpty)
                change = _state.GetStateChange(next);

            PerformChange(change, next);
        }

        private void ProcessUtf8(byte next)
        {
            if (!_utf8.Process(next, out var result))
                return;

            _eventSink.OnPrint(new VTPrintAction(result));
            _state = VTParserState.Ground;
        }

        private void PerformChange(VTStateAction change, byte next)
        {
            if (change.State == VTParserState.Anywhere)
            {
                PerformAction(change, next);
                return;
            }

            PerformAction(new VTStateAction(_state).WithExitAction(), 0);
            PerformAction(change, next);
            PerformAction(change.WithEntryAction(), 0);
            if (change.State != VTParserState.Anywhere)
                _state = change.State;
        }

        private void PerformAction(VTStateAction change, byte next)
        {
            if (change.Action == VTParserAction.None)
                return;

            System.Diagnostics.Debug.WriteLine(change);

            var (_, action) = change;

            switch (action)
            {
                case VTParserAction.Print:
                    _eventSink.OnPrint(new VTPrintAction(_utf8.Provide(next)));
                    return;
                case VTParserAction.Execute:
                    _eventSink.OnExecute(new VTExecuteAction((VTControlCode)next));
                    return;
                case VTParserAction.Hook:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;
                        
                    _eventSink.OnHook(new VTHookAction(
                        _params.AsSpan(0, _numParams), 
                        _intermediates.AsSpan(0, _intermediateIndex), 
                        (VTIgnore)(byte)(_options & ParseOptions.Ignoring)));
                        
                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case VTParserAction.Put:
                    _eventSink.OnPut(new VTPutAction(next));
                    return;
                case VTParserAction.OscStart:
                    _oscIndex = 0;
                    _oscNumParams = 0;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case VTParserAction.OscPut:
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
                case VTParserAction.OscEnd:
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

                    _eventSink.OnOscDispatch(new VTOscDispatchAction(
                        _oscParams.AsSpan(0, _oscNumParams),
                        (VTIgnore)(byte)(_options & ParseOptions.Ignoring)));
                    return;
                case VTParserAction.Unhook:
                    _eventSink.OnUnhook(default);
                    return;
                case VTParserAction.CsiDispatch:
                    if (_options.HasFlag(ParseOptions.CollectingParameters))
                        _params[_numParams++] = _param;
                    
                    _eventSink.OnCsiDispatch(new VTCsiDispatchAction(
                        _intermediates.AsSpan(0, _intermediateIndex),
                        _params.AsSpan(0, _numParams),
                        (VTIgnore)(byte)(_options & ParseOptions.Ignoring),
                        (char)next
                    ));
                    
                    _numParams = 0;
                    _param = 0;
                    _options &= ~ParseOptions.CollectingParameters;
                    return;
                case VTParserAction.EscDispatch:
                    _eventSink.OnEscDispatch(new VTEscDispatchAction(
                        _intermediates.AsSpan(0, _intermediateIndex),
                        (VTIgnore)(byte)(_options & ParseOptions.Ignoring),
                        next
                    ));
                    return;
                case VTParserAction.Ignore:
                case VTParserAction.None:
                    return;
                case VTParserAction.Collect:
                    if (_intermediateIndex == _intermediates.Length)
                        _options |= ParseOptions.IgnoringIntermediates;
                    else
                        _intermediates[_intermediateIndex++] = next;
                    return;
                case VTParserAction.Param:
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
                case VTParserAction.Clear:
                    _intermediateIndex = 0;
                    _numParams = 0;
                    _options &= ~ParseOptions.IgnoringIntermediates;
                    _options &= ~ParseOptions.IgnoringParameters;
                    return;
                case VTParserAction.BeginUtf8:
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
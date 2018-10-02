using System;
using System.Runtime.InteropServices;

namespace TerminalVelocity.VT
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct VTStateAction : IEquatable<VTStateAction>
    {
        private readonly byte _payload;
        public VTParserState State => (VTParserState)(_payload & 0x0F);
        public VTParserAction Action => (VTParserAction)(_payload >> 4);
        public bool IsEmpty => _payload == 0;

        public VTStateAction(VTParserState state, VTParserAction action) 
        {
            _payload = (byte)(
                ((byte)action << 4) |
                ((byte)state & 0x0F));
        }
        
        public VTStateAction(VTParserState state)
            : this(state, VTParserAction.None)
        {

        }
        
        public VTStateAction(VTParserAction action)
            : this(VTParserState.Anywhere, action)
        {

        }

        public VTStateAction(byte payload) => _payload = payload;

        public void Deconstruct(out VTParserState state, out VTParserAction action) => (state, action) = (State, Action);
        
        public void Deconstruct(out byte payload) => payload = _payload;

        public byte ToByte() => _payload;

        public VTStateAction WithState(VTParserState state) => new VTStateAction(state, Action);
        
        public VTStateAction WithAction(VTParserAction action) => new VTStateAction(State, action);
        
        public VTStateAction WithPayload(byte payload) => new VTStateAction(payload);

        public override bool Equals(object obj) => obj is VTStateAction o && Equals(o);

        public bool Equals(VTStateAction other) => other._payload == _payload;

        public override int GetHashCode() => HashCode.Combine(_payload);

        public override string ToString() => FormattableString.Invariant($"{State} {Action}");

        public static implicit operator VTStateAction((VTParserState, VTParserAction) tuple)
            => new VTStateAction(tuple.Item1, tuple.Item2);
    }
}
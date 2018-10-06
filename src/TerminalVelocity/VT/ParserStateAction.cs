using System;
using System.Runtime.InteropServices;

namespace TerminalVelocity.VT
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct ParserStateAction
    {
        private readonly byte _payload;
        public ParserState State => (ParserState)(_payload & 0x0F);
        public ParserAction Action => (ParserAction)(_payload >> 4);
        public bool IsEmpty => _payload == 0;

        public ParserStateAction(ParserState state, ParserAction action)
            => _payload = (byte)(((byte)action << 4) | ((byte)state & 0x0F));

        public ParserStateAction(ParserState state)
            : this(state, ParserAction.None)
        {

        }

        public ParserStateAction(ParserAction action)
            : this(ParserState.Anywhere, action)
        {

        }

        public void Deconstruct(out ParserState state, out ParserAction action) => (state, action) = (State, Action);

        public ParserStateAction WithAction(ParserAction action) => new ParserStateAction(State, action);

        public override string ToString() => FormattableString.Invariant($"{State} {Action}");
    }
}

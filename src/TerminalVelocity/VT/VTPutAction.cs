using System;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTPutAction
    {
        public byte Byte { get; }

        public VTPutAction(byte @byte) 
        { 
            Byte = @byte;
        }
        
        public override string ToString() => FormattableString.Invariant($"Put {(int)Byte:x2}");
    }
}
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
        
        public override string ToString() => FormattableString.Invariant($"Print 0x{(int)Byte:x2}");
    }
}
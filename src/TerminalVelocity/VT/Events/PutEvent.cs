using System;

namespace TerminalVelocity.VT.Events
{
    public readonly struct PutEvent
    {
        public const string ContractName = "Put.DCS.Events.VT.TerminalVelocity";

        public readonly byte Byte;

        public PutEvent(byte @byte) 
        { 
            Byte = @byte;
        }
        
        public override string ToString() => ((char)Byte).ToString();
    }
}
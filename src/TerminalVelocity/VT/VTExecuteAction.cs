using System;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTExecuteAction
    {
        public VTControlCode ControlCode { get; }

        public VTExecuteAction(VTControlCode controlCode) 
        {
            ControlCode = controlCode;
        }

        public override string ToString() => FormattableString.Invariant($"Execute {ControlCode}");
    }
}
using System;

namespace TerminalVelocity.VT.Events
{
    public readonly struct ExecuteEvent
    {
        public const string ContractName = "Execute.Events.VT.TerminalVelocity";

        public readonly ControlCode ControlCode;

        public ExecuteEvent(ControlCode controlCode) 
        {
            ControlCode = controlCode;
        }

        public override string ToString() => FormattableString.Invariant($"{ControlCode}");
    }
}
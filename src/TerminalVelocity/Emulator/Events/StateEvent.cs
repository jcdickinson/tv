using System;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct StateEvent
    {
        public const string ContractName = "State.Events.Emulator.TerminalVelocity";

        public readonly States States;

        public readonly StateMode Mode;

        public StateEvent(StateMode mode, States states)
        {
            States = states;
            Mode = mode;
        }
        
        public override string ToString() => FormattableString.Invariant($"{Mode} {States}");
    }
}
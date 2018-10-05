using System;

namespace TerminalVelocity.Emulator.Events
{
    public readonly struct MoveCursorEvent
    {
        public const string ContractName = "MoveCursor.Events.Emulator.TerminalVelocity";
        
        public readonly MoveOrigin Origin;
        public readonly MoveAxis Axis;
        public readonly int Count;

        public MoveCursorEvent(MoveOrigin origin, MoveAxis axis, int count)
        {
            Origin = origin;
            Axis = axis;
            Count = count;
        }

        public override string ToString() => FormattableString.Invariant($"{Origin} {Axis} {Count}");
    }
}
using System;

namespace TerminalVelocity.Emulator
{
    [Flags]
    public enum States : int
    {
        None = 0,
        All = -1,
        KeypadApplicationMode = 0x1
    }
}

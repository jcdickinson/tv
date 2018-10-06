namespace TerminalVelocity.Emulator
{
    public enum MoveOrigin : byte
    {
        Absolute = 0,
        Relative = 1,
        Inverse = 2,

        Store = 3,
        Restore = 4
    }
}

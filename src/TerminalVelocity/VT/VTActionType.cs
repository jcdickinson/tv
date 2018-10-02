namespace TerminalVelocity.VT
{
    public enum VTActionType : byte
    {
        None = 0,
        Print,
        Execute,
        Hook,
        Put,
        Unhook,
        OscDispatch,
        CsiDispatch,
        EscDispatch
    }
}
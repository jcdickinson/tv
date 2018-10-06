namespace TerminalVelocity.VT
{
    internal enum ParserAction : byte
    {
        None = 0,
        Clear = 1,
        Collect = 2,
        CsiDispatch = 3,
        EscDispatch = 4,
        Execute = 5,
        Hook = 6,
        Ignore = 7,
        OscEnd = 8,
        OscPut = 9,
        OscStart = 10,
        Param = 11,
        Print = 12,
        Put = 13,
        Unhook = 14,
        BeginUtf8 = 15,
    }
}

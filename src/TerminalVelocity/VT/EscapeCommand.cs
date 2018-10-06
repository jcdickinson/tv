namespace TerminalVelocity.VT
{
    public enum EscapeCommand : byte
    {
        ConfigureAsciiCharSet = (byte)'B',
        ConfigureSpecialCharSet = (byte)'0',
        LineFeed = (byte)'D',
        NextLine = (byte)'E',
        SetHorizontalTabStop = (byte)'H',
        ReverseIndex = (byte)'M',
        IdentifyTerminal = (byte)'Z',
        SaveCursorPosition = (byte)'7',
        DecTest = (byte)'8',
        RestoreCursorPosition = DecTest,
        SetKeypadApplicationMode = (byte)'=',
        UnsetKeypadApplicationMode = (byte)'>',
        StringTerminator = (byte)'\\',
        ResetState = (byte)'c'
    }
}

namespace TerminalVelocity.VT
{
    public enum VTMode
    {
        CursorKeys = 1,
        DECCOLM = 3,
        Insert = 4,
        Origin = 6,
        LineWrap = 7,
        BlinkingCursor = 12,
        LineFeedNewLine = 20,
        ShowCursor = 25,
        ReportMouseClicks = 1000,
        ReportCellMouseMotion = 1002,
        ReportAllMouseMotion = 1003,
        ReportFocusInOut = 1004,
        SgrMouse = 1006,
        SwapScreenAndSetRestoreCursor = 1049,
        BracketedPaste = 2004,
    }
}
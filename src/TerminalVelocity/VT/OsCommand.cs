namespace TerminalVelocity.VT
{
    public enum OsCommand : short
    {
        SetWindowIconAndTitle = 0,
        SetWindowIcon = 1,
        SetWindowTitle = 2,
        SetColor = 4,
        SetForegroundColor = 10,
        SetBackgroundColor = 11,
        SetCursorColor = 12,
        SetCursorStyle = 50,
        SetClipboard = 52,
        ResetColor = 104,
        ResetForegroundColor = 110,
        ResetBackgroundColor = 111,
        ResetCursorColor = 112,
        Unknown = -1
    }
}

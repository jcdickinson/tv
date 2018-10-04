namespace TerminalVelocity.Emulator
{
    internal enum EscapeCommand : byte
    {
        ConfigureAsciiCharSet = (byte)'B',
        ConfigureSpecialCharacterAndLineDrawingCharSet = (byte)'0'
    }
}
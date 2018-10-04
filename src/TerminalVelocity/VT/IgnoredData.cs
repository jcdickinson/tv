using System;

namespace TerminalVelocity.VT
{
    [Flags]
    public enum IgnoredData : byte
    {
        None = 0,
        Intermediates = 0b0000_00001,
        Parameters = 0b0000_00010,
        All = Intermediates | Parameters
    }
}
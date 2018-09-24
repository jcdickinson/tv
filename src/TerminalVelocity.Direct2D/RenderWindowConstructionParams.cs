using System;
using System.Composition;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared, Export(typeof(IConstructionParams))]
    public sealed class RenderWindowConstructionParams : IConstructionParams
    {
        public WindowStyles Styles => WindowStyles.WS_OVERLAPPEDWINDOW;

        public WindowExStyles ExStyles => WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_NOREDIRECTIONBITMAP;

        public uint ControlStyles => 0;

        public int Width => 500;

        public int Height => 500;

        public int X => 10;

        public int Y => 10;

        public IntPtr ParentHandle => IntPtr.Zero;

        public IntPtr MenuHandle => IntPtr.Zero;
    }
}
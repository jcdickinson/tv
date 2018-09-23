using System;
using NetCoreEx.BinaryExtensions;
using NetCoreEx.Geometry;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public unsafe struct NcButtonPacket
    {
        public unsafe WindowMessage* Message;

        public NcButtonPacket(WindowMessage* message)
        {
            Message = message;
        }

        public IntPtr Hwnd { get { return this.Message->Hwnd; } set { this.Message->Hwnd = value; } }

        public WinApi.User32.HitTestResult HitTestResult
        {
            get { return (WinApi.User32.HitTestResult)this.Message->WParam.ToSafeInt32(); }
            set { this.Message->WParam = new IntPtr((int)value); }
        }

        public Point Point
        {
            get
            {
                Point point;
                this.Message->LParam.BreakSafeInt32To16Signed(out point.Y, out point.X);
                return point;
            }

            set { this.Message->LParam = new IntPtr(value.ToInt32()); }
        }
    }
}
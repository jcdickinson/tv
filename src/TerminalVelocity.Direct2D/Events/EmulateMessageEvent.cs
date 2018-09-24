using System;
using SharpDX;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    public struct EmulateMessageEvent
    {
        public const string ContractName = "EmulateMessage.Events.Direct2D.TerminalVelocity";

        public readonly WindowMessage Message;
        public IntPtr Result;

        public EmulateMessageEvent(in WindowMessage message)
        {
            Message = message;
            Result = default;
        }

        public static unsafe EmulateMessageEvent CreateSysCommand(
            SysCommand command,
            bool isAccelerator = false,
            bool isMnemonic = false,
            short x = 0,
            short y = 0)
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.SYSCOMMAND, IntPtr.Zero, IntPtr.Zero);
            var packet = new SysCommandPacket(&message);
            packet.Command = command;
            packet.IsAccelerator = isAccelerator;
            packet.IsMnemonic = isMnemonic;
            packet.X = x;
            packet.Y = y;
            return new EmulateMessageEvent(message);
        }

        public static unsafe EmulateMessageEvent CreateQuit(int code = 0)
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.QUIT, IntPtr.Zero, IntPtr.Zero);
            var packet = new QuitPacket(&message);
            packet.Code = code;
            return new EmulateMessageEvent(message);
        }

        public static unsafe EmulateMessageEvent CreateClose()
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
            return new EmulateMessageEvent(message);
        }

        public static unsafe EmulateMessageEvent CreatePaint()
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.PAINT, IntPtr.Zero, IntPtr.Zero);
            return new EmulateMessageEvent(message);
        }
    }
}
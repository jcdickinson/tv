using System;
using TerminalVelocity.Eventing;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    [Event]
    public sealed class EmulateMessageEvent : Event<UIEventLoop, EmulateMessageEventData>
    {
        public EmulateMessageEvent(UIEventLoop eventLoop) : base(eventLoop) { }
    }

    public struct EmulateMessageEventData
    {
        public readonly WindowMessage Message;

        public EmulateMessageEventData(in WindowMessage message) => Message = message;

        public static unsafe EmulateMessageEventData CreateSysCommand(
            SysCommand command,
            bool isAccelerator = false,
            bool isMnemonic = false,
            short x = 0,
            short y = 0)
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.SYSCOMMAND, IntPtr.Zero, IntPtr.Zero);
            var packet = new SysCommandPacket(&message)
            {
                Command = command,
                IsAccelerator = isAccelerator,
                IsMnemonic = isMnemonic,
                X = x,
                Y = y
            };
            return new EmulateMessageEventData(message);
        }

        public static unsafe EmulateMessageEventData CreateQuit(int code = 0)
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.QUIT, IntPtr.Zero, IntPtr.Zero);
            var packet = new QuitPacket(&message)
            {
                Code = code
            };
            return new EmulateMessageEventData(message);
        }

        public static unsafe EmulateMessageEventData CreateClose()
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
            return new EmulateMessageEventData(message);
        }

        public static unsafe EmulateMessageEventData CreatePaint()
        {
            var message = new WindowMessage(IntPtr.Zero, (uint)WM.PAINT, IntPtr.Zero, IntPtr.Zero);
            return new EmulateMessageEventData(message);
        }

        public override string ToString()
            => FormattableString.Invariant($"{Message.Hwnd}.{Message.Id} ({Message.LParam}, {Message.WParam})");
    }
}

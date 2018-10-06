using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using TerminalVelocity.Eventing;
using WinApi.User32;

namespace TerminalVelocity.Direct2D
{
    public sealed class UIEventLoop : EventLoop
    {
        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            public const string User32 = "User32.DLL";

            [DllImport(User32, SetLastError = true, CharSet = CharSet.Auto)]
            private static extern uint RegisterWindowMessage(string name);

            public static Message CreateMessage()
            {
                // Generate random+unique values because misbehaving processes may send junk.

                var message = new Message()
                {
                    Value = NativeMethods.RegisterWindowMessage("Post.UIEventLoop.Direct2D.TerminalVelocity")
                };

                var buffer = new Span<byte>(new byte[IntPtr.Size]);
                if (Environment.Is64BitProcess)
                {
                    // RandomNumberGenerator is just a convenience, not that paranoid :)
                    RandomNumberGenerator.Fill(buffer);
                    MemoryMarshal.Cast<byte, int>(buffer)[0] = Environment.TickCount;
                    message.WParam = MemoryMarshal.Cast<byte, IntPtr>(buffer)[0];
                    RandomNumberGenerator.Fill(buffer);
                    message.LParam = MemoryMarshal.Cast<byte, IntPtr>(buffer)[0];
                }
                else
                {
                    RandomNumberGenerator.Fill(buffer);
                    message.WParam = new IntPtr(Environment.TickCount);
                    message.LParam = MemoryMarshal.Cast<byte, IntPtr>(buffer)[0];
                }

                return message;
            }
        }


        public override int Priority => int.MaxValue;

        private readonly Message _eventLoopMessage;
        private readonly RenderWindow _renderWindow;

        public UIEventLoop(
            RenderWindow renderWindow)
        {
            _renderWindow = renderWindow;
            _eventLoopMessage = NativeMethods.CreateMessage();
        }

        protected override void OnEventPublished<T>(T e) => User32Methods.SendMessage(
                _renderWindow.Handle,
                _eventLoopMessage.Value,
                _eventLoopMessage.WParam,
                _eventLoopMessage.LParam);

        public override void Execute()
        {
            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);
            _renderWindow.Show();

            var result = 0;
            while (IsRunning && (result = User32Methods.GetMessage(out Message message, _renderWindow.Handle, 0, 0)) > 0)
            {
                User32Methods.TranslateMessage(ref message);
                User32Methods.DispatchMessage(ref message);

                if (message.Value == _eventLoopMessage.Value &&
                    message.WParam == _eventLoopMessage.WParam &&
                    message.LParam == _eventLoopMessage.LParam)
                    ExecuteEvents();

                if (message.Value == (uint)WM.QUIT)
                {
                    result = 0;
                    break;
                }
            }
        }
    }
}

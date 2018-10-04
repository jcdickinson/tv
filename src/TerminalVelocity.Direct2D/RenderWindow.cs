using System;
using System.Collections.Generic;
using System.Composition;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.DxUtils.Component;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public sealed class RenderWindow : EventedWindowCore
    {
        [Import(MouseButtonEvent.ContractName)]
        public Event<MouseButtonEvent> MouseButton { private get; set; }

        [Import(RenderEvent.ContractName)]
        public Event<RenderEvent> Render { private get; set; }

        [Import(SysCommandEvent.ContractName)]
        public Event<SysCommandEvent> SysCommand { private get; set; }

        [Import(SizeEvent.ContractName)]
        public Event<SizeEvent> Size { private get; set; }

        [Import(CloseEvent.ContractName)]
        public Event<CloseEvent> Close { private get; set; }

        private readonly Dx11Component _directX;

        public RenderWindow(
            Dx11Component directX)
        {
            _directX = directX;
        }

        [Import(EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> EmulateMessage 
        {
            set => value.Subscribe((ref EmulateMessageEvent message) =>
            {
                message.Result = User32Methods.SendMessage(
                    Handle, 
                    (uint)message.Message.Id, 
                    message.Message.WParam, 
                    message.Message.WParam);
            });
        }

        protected override void OnMessage(ref WindowMessage msg)
        {
            //Console.WriteLine(msg.Id);
            base.OnMessage(ref msg);
        }

        protected override void OnSysCommand(ref SysCommandPacket packet)
        {
            var evt = new SysCommandEvent(packet);
            if (!SysCommand.Publish(ref evt))
                base.OnSysCommand(ref packet);
        }

        protected override void OnSize(ref SizePacket packet)
        {
            var evt = new SizeEvent(packet);
            if (!Size.Publish(ref evt))
                base.OnSize(ref packet);
        }
        
        protected override void OnClose(ref Packet packet)
        {
            var evt = new CloseEvent();
            if (!Close.Publish(ref evt))
                base.OnClose(ref packet);
        }

        protected override void OnCreate(ref CreateWindowPacket packet)
        {
            _directX.Initialize(Handle, GetClientSize());
            RedrawFrame();
            SetText("Terminal Velocity");

            base.OnCreate(ref packet);
        }

        protected override void OnPaint(ref PaintPacket packet)
        {
            Render.Publish(new RenderEvent());
            Validate();
        }
    }
}
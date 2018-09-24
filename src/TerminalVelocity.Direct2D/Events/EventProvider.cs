using System.Composition;
using SharpDX;

namespace TerminalVelocity.Direct2D.Events
{
    [Shared]
    public sealed class EventProvider
    {
        [Export(Events.CloseEvent.ContractName)]
        public Event<CloseEvent> CloseEvent { get; } = new Event<CloseEvent>();
        [Export(Events.HitTestEvent.ContractName)]
        public Event<HitTestEvent> HitTestEvent { get; } = new Event<HitTestEvent>();
        
        [Export(Events.LayoutEvent.ContractName)]
        public Event<LayoutEvent> LayoutEvent  { get; } = new Event<LayoutEvent>();
        
        [Export(Events.MouseButtonEvent.ContractName)]
        public Event<MouseButtonEvent> MouseButtonEvent { get; } = new Event<MouseButtonEvent>();
        
        [Export(Events.RenderEvent.ContractName)]
        public Event<RenderEvent> RenderEvent { get; } = new Event<RenderEvent>();
        
        [Export(Events.SizeEvent.ContractName)]
        public Event<SizeEvent> SizeEvent { get; } = new Event<SizeEvent>();
        
        [Export(Events.SysCommandEvent.ContractName)]
        public Event<SysCommandEvent> SysCommandEvent { get; } = new Event<SysCommandEvent>();
        
        [Export(Events.EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> EmulateMessageEvent { get; } = new Event<EmulateMessageEvent>();
    }
}
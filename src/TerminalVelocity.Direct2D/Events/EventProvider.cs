using System.Composition;
using SharpDX;

namespace TerminalVelocity.Direct2D.Events
{
    [Shared]
    public sealed class EventProvider
    {
        [Export(Events.CloseEvent.ContractName)]
        public Event<CloseEvent> CloseEvent { get; } = new Event<CloseEvent>(Events.CloseEvent.ContractName);
        
        [Export(Events.MouseButtonEvent.ContractName)]
        public Event<MouseButtonEvent> MouseButtonEvent { get; } = new Event<MouseButtonEvent>(Events.MouseButtonEvent.ContractName);
        
        [Export(Events.RenderEvent.ContractName)]
        public Event<RenderEvent> RenderEvent { get; } = new Event<RenderEvent>(Events.RenderEvent.ContractName);
        
        [Export(Events.SizeEvent.ContractName)]
        public Event<SizeEvent> SizeEvent { get; } = new Event<SizeEvent>(Events.SizeEvent.ContractName);
        
        [Export(Events.SysCommandEvent.ContractName)]
        public Event<SysCommandEvent> SysCommandEvent { get; } = new Event<SysCommandEvent>(Events.SysCommandEvent.ContractName);
        
        [Export(Events.EmulateMessageEvent.ContractName)]
        public Event<EmulateMessageEvent> EmulateMessageEvent { get; } = new Event<EmulateMessageEvent>(Events.EmulateMessageEvent.ContractName);
    }
}
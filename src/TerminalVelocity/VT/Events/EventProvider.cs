using System.Composition;

namespace TerminalVelocity.VT.Events
{
    [Shared]
    public sealed class EventProvider
    {
        [Export(Events.ControlSequenceEvent.ContractName)]
        public Event<ControlSequenceEvent> ControlSequenceEvent { get; } = new Event<ControlSequenceEvent>();
        
        [Export(Events.EscapeSequenceEvent.ContractName)]
        public Event<EscapeSequenceEvent> EscapeSequenceEvent { get; } = new Event<EscapeSequenceEvent>();
        
        [Export(Events.ExecuteEvent.ContractName)]
        public Event<ExecuteEvent> ExecuteEvent { get; } = new Event<ExecuteEvent>();
        
        [Export(Events.HookEvent.ContractName)]
        public Event<HookEvent> HookEvent { get; } = new Event<HookEvent>();
        
        [Export(Events.OsCommandEvent.ContractName)]
        public Event<OsCommandEvent> OsCommandEvent { get; } = new Event<OsCommandEvent>();
        
        [Export(Events.PrintEvent.ContractName)]
        public Event<PrintEvent> PrintEvent { get; } = new Event<PrintEvent>();
        
        [Export(Events.PutEvent.ContractName)]
        public Event<PutEvent> PutEvent { get; } = new Event<PutEvent>();
        
        [Export(Events.UnhookEvent.ContractName)]
        public Event<UnhookEvent> UnhookEvent { get; } = new Event<UnhookEvent>();
    }
}
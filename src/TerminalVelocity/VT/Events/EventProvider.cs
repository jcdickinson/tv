using System.Composition;

namespace TerminalVelocity.VT.Events
{
    [Shared]
    public sealed class EventProvider
    {
        [Export(Events.ControlSequenceEvent.ContractName)]
        public Event<ControlSequenceEvent> ControlSequenceEvent { get; } = new Event<ControlSequenceEvent>(Events.ControlSequenceEvent.ContractName);
        
        [Export(Events.EscapeSequenceEvent.ContractName)]
        public Event<EscapeSequenceEvent> EscapeSequenceEvent { get; } = new Event<EscapeSequenceEvent>(Events.EscapeSequenceEvent.ContractName);
        
        [Export(Events.ExecuteEvent.ContractName)]
        public Event<ExecuteEvent> ExecuteEvent { get; } = new Event<ExecuteEvent>(Events.ExecuteEvent.ContractName);
        
        [Export(Events.HookEvent.ContractName)]
        public Event<HookEvent> HookEvent { get; } = new Event<HookEvent>(Events.HookEvent.ContractName);
        
        [Export(Events.OsCommandEvent.ContractName)]
        public Event<OsCommandEvent> OsCommandEvent { get; } = new Event<OsCommandEvent>(Events.OsCommandEvent.ContractName);
        
        [Export(Events.PrintEvent.ContractName)]
        public Event<PrintEvent> PrintEvent { get; } = new Event<PrintEvent>(Events.PrintEvent.ContractName);
        
        [Export(Events.PutEvent.ContractName)]
        public Event<PutEvent> PutEvent { get; } = new Event<PutEvent>(Events.PutEvent.ContractName);
        
        [Export(Events.UnhookEvent.ContractName)]
        public Event<UnhookEvent> UnhookEvent { get; } = new Event<UnhookEvent>(Events.UnhookEvent.ContractName);
    }
}
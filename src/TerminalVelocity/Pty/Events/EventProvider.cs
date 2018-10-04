using System.Composition;

namespace TerminalVelocity.Pty.Events
{
    [Shared]
    public sealed class EventProvider
    {
        [Export(Events.ReceiveEvent.ContractName)]
        public Event<ReceiveEvent> ReceiveEvent { get; } = new Event<ReceiveEvent>(Events.ReceiveEvent.ContractName);
    }
}
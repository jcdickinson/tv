namespace TerminalVelocity.Eventing
{
    internal interface IEvent
    {
        EventStatus PublishEvent(ulong eventId);
    }
}

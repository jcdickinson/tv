namespace TerminalVelocity.Eventing
{
    public delegate EventStatus EventSubscriber<T>(in T e)
        where T : struct;
}

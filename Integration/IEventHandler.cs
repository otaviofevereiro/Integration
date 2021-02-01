namespace Integration.Core
{
    public interface IEventHandler<in TEvent>: IRawEventHandler<TEvent>
        where TEvent : Event
    {
    }
}

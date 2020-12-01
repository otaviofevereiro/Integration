namespace Integration
{
    public interface IEventBus
    {
        public void Publish(string eventName, object message);

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}

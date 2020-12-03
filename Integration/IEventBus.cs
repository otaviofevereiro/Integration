using System.Threading.Tasks;

namespace Integration
{
    public interface IEventBus
    {
        public Task Publish(string eventName, object message);

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}

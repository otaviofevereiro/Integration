using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IEventBus
    {
        public Task Publish(string eventName, object message);

        public Task Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>;

    }
}

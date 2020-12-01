using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration
{
    public abstract class EventBus : IEventBus
    {
        private readonly Subscriber subscriber;

        protected EventBus(Subscriber subscriber)
        {
            this.subscriber = subscriber;
        }

        public abstract void Publish(string eventName, object message);

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            subscriber.Add<TEvent, TEventHandler>();

            DoSubscribe(subscriber.GetEventName<TEvent>());
        }

        protected abstract void DoSubscribe(string eventName);

        protected async Task Notify(string eventName, byte[] message)
        {
            var eventsTypes = subscriber.GetEventsTypesByName(eventName);

            foreach (var eventType in eventsTypes)
            {
                var @event = JsonConvert.DeserializeObject(message, eventType);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                await Task.Yield();
                await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
            }
        }
    }
}

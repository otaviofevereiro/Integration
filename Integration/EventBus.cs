using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Integration
{
    public abstract class EventBus : IEventBus
    {
        private readonly Subscriber subscriber;
        private readonly IServiceProvider serviceProvider;

        protected EventBus(Subscriber subscriber, IServiceProvider serviceProvider)
        {
            this.subscriber = subscriber;
            this.serviceProvider = serviceProvider;
        }

        public abstract Task Publish(string eventName, object message);

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
            var tasks = GetNotifyTasks(eventName, message);

            await Task.WhenAll(tasks);
        }

        private IEnumerable<Task> GetNotifyTasks(string eventName, byte[] message)
        {
            var subscriberInfos = subscriber.GetEventHandlersTypesByName(eventName);

            foreach (var subscriberInfo in subscriberInfos)
            {
                var eventHandlers = serviceProvider.GetServices(subscriberInfo.EventHandlerType);

                //TODO:Converter para Newtonsoft
                var @event = JsonSerializer.Deserialize(message, subscriberInfo.EventType);

                foreach (var eventHandler in eventHandlers)
                {
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(subscriberInfo.EventType);

                    yield return (Task)concreteType.InvokeMember("Handle", 
                                                                 BindingFlags.InvokeMethod, 
                                                                 null,
                                                                 eventHandler, 
                                                                 new object[] { @event });
                }
            }
        }
    }
}

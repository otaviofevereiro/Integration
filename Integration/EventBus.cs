using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Integration.Core
{
    public abstract class EventBus : IEventBus
    {
        private readonly IServiceProvider serviceProvider;
        protected readonly SubscriberManager subscriber;

        protected EventBus(SubscriberManager subscriber, IServiceProvider serviceProvider)
        {
            this.subscriber = subscriber;
            this.serviceProvider = serviceProvider;
        }

        public abstract Task Publish(string eventName, object message);

        public async Task Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>
        {
            subscriber.Add<TEvent, TEventHandler>();

            await DoSubscribe(subscriber.GetEventName<TEvent>());
        }

        protected abstract Task DoSubscribe(string eventName);

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
                object @event;

                if (!eventHandlers.Any())
                    break;

                using (var sr = new StreamReader(new MemoryStream(message)))
                {
                    string jsonMessage = sr.ReadToEnd();
                    @event = JsonConvert.DeserializeObject(jsonMessage, subscriberInfo.EventType);
                }

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

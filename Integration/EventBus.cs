using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public abstract class EventBus : IPublisher, ISubscriber
    {
        protected readonly SubscriberManager _subscriber;
        private readonly ILogger<EventBus> _logger;
        private readonly IServiceProvider _serviceProvider;

        protected EventBus(SubscriberManager subscriber, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _subscriber = subscriber;
            _serviceProvider = serviceProvider;
            _logger = loggerFactory.CreateLogger<EventBus>();
        }

        public abstract Task Publish(string eventName, object @event, CancellationToken cancellationToken = default);

        public async Task Subscribe<TEvent, TEventHandler>(CancellationToken cancellationToken = default)
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>
        {
            _logger.LogDebug("Adding new subscribtion on EventBus.");

            _subscriber.Add<TEvent, TEventHandler>();

            string eventName = _subscriber.GetEventName<TEvent>();

            await DoSubscribe(eventName, cancellationToken);

            _logger.LogInformation($"Has been added a new event '{eventName}' on EventBus.");
        }

        protected abstract Task DoSubscribe(string eventName, CancellationToken cancellationToken);

        protected async Task Notify(string eventName, byte[] eventBytes)
        {
            var tasks = GetNotifyTasks(eventName, eventBytes);

            _logger.LogDebug($"Notifying handlers with new message of event '{eventName}'.");

            await Task.WhenAll(tasks);
        }

        private object Deserialize(byte[] eventBytes, Type type)
        {
            _logger.LogDebug($"Deserializing event.");

            using (var sr = new StreamReader(new MemoryStream(eventBytes)))
            {
                string eventJson = sr.ReadToEnd();
                var message = JsonConvert.DeserializeObject(eventJson, type);

                _logger.LogDebug($"Event has been deserialize.");

                return message;
            }
        }

        private IEnumerable<Task> GetNotifyTasks(string eventName, byte[] eventBytes)
        {
            var subscriberInfos = _subscriber.GetSubscribersInfo(eventName);

            foreach (var subscriberInfo in subscriberInfos)
            {
                _logger.LogDebug($"Getting registered handlers '{subscriberInfo.EventHandlerType}'.");
                var eventHandlers = _serviceProvider.GetServices(subscriberInfo.EventHandlerType);

                if (!eventHandlers.Any())
                {
                    _logger.LogDebug($"No registers of handlers found to '{subscriberInfo.EventHandlerType}'.");

                    break;
                }

                var @event = Deserialize(eventBytes, subscriberInfo.EventType);

                //TODO:Complete Ack or Nack message or send to deadletter
                //Notification<Event> notification = new()
                //{
                //    Event = new Event()
                //};

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

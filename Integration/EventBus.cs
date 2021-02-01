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
        public abstract Task Publish(string eventName, IEnumerable<object> events, CancellationToken cancellationToken = default);

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>
        {
            _logger.LogDebug("Adding new subscribtion on EventBus.");

            _subscriber.Add<TEvent, TEventHandler>();

            string eventName = _subscriber.GetEventName<TEvent>();

            DoSubscribe(eventName);

            _logger.LogInformation($"Has been added a new event '{eventName}' on EventBus.");
        }

        protected abstract void DoSubscribe(string eventName);

        protected async Task Notify(IEventContext eventContext)
        {
            var tasks = GetNotifyTasks(eventContext);

            _logger.LogDebug($"Notifying handlers with new message of event '{eventContext.EventName}'.");

            await Task.WhenAll(tasks);
        }

        private object Deserialize(ReadOnlyMemory<byte> eventBytes, Type type)
        {
            _logger.LogDebug($"Deserializing event.");

            using (var sr = new StreamReader(new MemoryStream(eventBytes.ToArray())))
            {
                string eventJson = sr.ReadToEnd();
                var message = JsonConvert.DeserializeObject(eventJson, type);

                _logger.LogDebug($"Event has been deserialize.");

                return message;
            }
        }

        private IEnumerable<Task> GetNotifyTasks(IEventContext eventContext)
        {
            var subscriberInfos = _subscriber.GetSubscribersInfo(eventContext.EventName);

            foreach (var subscriberInfo in subscriberInfos)
            {
                _logger.LogDebug($"Getting registered handlers '{subscriberInfo.EventHandlerType}'.");
                var eventHandlers = _serviceProvider.GetServices(subscriberInfo.EventHandlerType);

                if (!eventHandlers.Any())
                {
                    _logger.LogDebug($"No registers of handlers found to '{subscriberInfo.EventHandlerType}'.");

                    break;
                }

                var @event = Deserialize(eventContext.Event, subscriberInfo.EventType);

                foreach (var eventHandler in eventHandlers)
                {
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(subscriberInfo.EventType);

                    yield return (Task)concreteType.InvokeMember("Handle",
                                                                 BindingFlags.InvokeMethod,
                                                                 null,
                                                                 eventHandler,
                                                                 new object[] { @event, eventContext });
                }
            }
        }
    }
}

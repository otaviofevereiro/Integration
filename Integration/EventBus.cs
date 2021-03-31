using Integration.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public abstract Task Publish<TEvent>(string eventName, TEvent @event, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default);
        public abstract Task Publish<TEvent>(string eventName, IEnumerable<TEvent> events, CancellationToken cancellationToken = default);
        public abstract Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : Event;
        public abstract Task Publish<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
            where TEvent : Event;

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


        protected async Task<bool> Notify(string id, string eventName, ReadOnlyMemory<byte> @event, IDictionary<string, object> properties)
        {
            var subscriberInfos = _subscriber.GetSubscribersInfo(eventName);
            var eventsContexts = new List<EventContext>();

            foreach (var subscriberInfo in subscriberInfos)
            {
                _logger.LogDebug($"Getting registered handlers '{subscriberInfo.EventHandlerType}'.");
                var eventHandlers = _serviceProvider.GetServices(subscriberInfo.EventHandlerType);

                if (!eventHandlers.Any())
                {
                    _logger.LogDebug($"No registers of handlers found to '{subscriberInfo.EventHandlerType}'.");

                    break;
                }

                var tasks = new List<Task>();

                foreach (var eventHandler in eventHandlers)
                {
                    var eventContext = (EventContext)Activator.CreateInstance(typeof(EventContext<>).MakeGenericType(subscriberInfo.EventType),
                                                                              new object[] { id, eventName, @event, properties });

                    var concreteType = typeof(IEventHandler<>).MakeGenericType(subscriberInfo.EventType);

                    tasks.Add((Task)concreteType.InvokeMember("Handle",
                                                              BindingFlags.InvokeMethod,
                                                              null,
                                                              eventHandler,
                                                              new object[] { eventContext }));

                    eventsContexts.Add(eventContext);
                }

                await Task.WhenAll(tasks);
            }

            return eventsContexts.All(c => c.Completed);
        }
    }
}



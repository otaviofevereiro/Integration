using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration.Core
{
    public class SubscriberManager
    {
        private readonly Dictionary<string, List<SubscriberInfo>> _handlers;
        private readonly ILogger<SubscriberManager> _logger;

        public SubscriberManager(ILoggerFactory loggerFactory)
        {
            _handlers = new Dictionary<string, List<SubscriberInfo>>();
            _logger = loggerFactory.CreateLogger<SubscriberManager>();
        }

        public IReadOnlyDictionary<string, List<SubscriberInfo>> Handlers => _handlers;

        public void Add<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            Add(new SubscriberInfo(typeof(TEvent), typeof(TEventHandler)));
        }

        public void Add(SubscriberInfo subscriberInfo)
        {
            _logger.LogInformation($"Adding new subscriber...");

            var eventTypeName = GetEventName(subscriberInfo.EventType);

            if (_handlers.ContainsKey(eventTypeName))
            {
                _logger.LogInformation($"There are events with event '{eventTypeName}' registered");

                var eventHandlersTypes = _handlers[eventTypeName];

                if (eventHandlersTypes.Any(info => info.EventHandlerType == subscriberInfo.EventHandlerType))
                    throw new InvalidOperationException($"Handler Type {subscriberInfo.EventHandlerType.Name} already registered for '{eventTypeName}'");

                _handlers[eventTypeName].Add(subscriberInfo);
                _logger.LogInformation($"Has been added new event handler '{subscriberInfo.EventHandlerType.Name}' to event '{eventTypeName}'");

            }
            else
            {
                _logger.LogInformation($"There are no events with name '{eventTypeName}' registered");

                _handlers.Add(eventTypeName, new List<SubscriberInfo>() { subscriberInfo });

                _logger.LogInformation($"A new new event handler '{subscriberInfo.EventHandlerType.Name}' has been added to event '{eventTypeName}'");
            }
        }

        internal List<SubscriberInfo> GetSubscribersInfo(string eventName)
        {
            _logger.LogDebug($"Getting subscribers information of event '{eventName}'.");

            return _handlers[eventName];
        }

        internal string GetEventName<TEvent>()
            where TEvent : Event
        {
            return GetEventName(typeof(TEvent));
        }

        internal string GetEventName(Type type)
        {
            return type.Name;
        }
    }
}

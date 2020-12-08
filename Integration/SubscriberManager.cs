using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration.Core
{
    public class SubscriberManager
    {
        private readonly Dictionary<string, List<SubscriberInfo>> handlers;

        public SubscriberManager()
        {
            handlers = new Dictionary<string, List<SubscriberInfo>>();
        }

        internal void Add<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var eventTypeName = GetEventName<TEvent>();
            var eventHandlerType = typeof(TEventHandler);

            if (handlers.ContainsKey(eventTypeName))
            {
                var eventHandlersTypes = handlers[eventTypeName];

                if (eventHandlersTypes.Any(info => info.EventHandlerType == eventHandlerType))
                    throw new InvalidOperationException($"Handler Type {eventHandlerType.Name} already registered for '{eventTypeName}'");

                handlers[eventTypeName].Add(new SubscriberInfo(eventType, eventHandlerType));
            }

            handlers.Add(eventTypeName, new List<SubscriberInfo>() { new SubscriberInfo(eventType, eventHandlerType) });
        }

        internal List<SubscriberInfo> GetEventHandlersTypesByName(string eventName)
        {
            return handlers[eventName];
        }

        internal string GetEventName<TEvent>()
            where TEvent : Event
        {
            return typeof(TEvent).Name;
        }
    }
}

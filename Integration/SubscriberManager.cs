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

        public IReadOnlyDictionary<string, List<SubscriberInfo>> Handlers => handlers;

        public void Add<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            Add(new SubscriberInfo(typeof(TEvent), typeof(TEventHandler)));
        }

        public void Add(SubscriberInfo subscriberInfo)
        {
            var eventTypeName = GetEventName(subscriberInfo.EventType);

            if (handlers.ContainsKey(eventTypeName))
            {
                var eventHandlersTypes = handlers[eventTypeName];

                if (eventHandlersTypes.Any(info => info.EventHandlerType == subscriberInfo.EventHandlerType))
                    throw new InvalidOperationException($"Handler Type {subscriberInfo.EventHandlerType.Name} already registered for '{eventTypeName}'");

                handlers[eventTypeName].Add(subscriberInfo);
            }
            else
            {
                handlers.Add(eventTypeName, new List<SubscriberInfo>() { subscriberInfo });
            }
        }

        internal List<SubscriberInfo> GetEventHandlersTypesByName(string eventName)
        {
            return handlers[eventName];
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

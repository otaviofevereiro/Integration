using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration
{
    public class Subscriber
    {
        private readonly Dictionary<string, List<Type>> handlers;

        public Subscriber()
        {
            handlers = new Dictionary<string, List<Type>>();
        }

        internal void Add<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventTypeName = GetEventName<TEvent>();
            var eventHandlerType = typeof(TEventHandler);

            if (handlers.ContainsKey(eventTypeName))
            {
                var eventHandlersTypes = handlers[eventTypeName];

                if (eventHandlersTypes.Any(type => type == eventHandlerType))
                    throw new InvalidOperationException($"Handler Type {eventHandlerType.Name} already registered for '{eventTypeName}'");

                handlers[eventTypeName].Add(eventHandlerType);
            }

            handlers.Add(eventTypeName, new List<Type>() { eventHandlerType });
        }

        internal List<Type> GetEventsTypesByName(string eventName)
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

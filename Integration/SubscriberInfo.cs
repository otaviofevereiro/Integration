using System;

namespace Integration.Core
{
    public class SubscriberInfo
    {
        public SubscriberInfo(Type eventType, Type eventHandlerType)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            EventHandlerType = eventHandlerType ?? throw new ArgumentNullException(nameof(eventHandlerType));
        }

        public Type EventType { get; }
        public Type EventHandlerType { get;}
    }
}

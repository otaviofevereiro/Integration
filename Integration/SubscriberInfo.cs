using System;

namespace Integration
{
    public class SubscriberInfo
    {
        public SubscriberInfo(Type eventType, Type eventHandlerType)
        {
            EventType = eventType;
            EventHandlerType = eventHandlerType;
        }

        public Type EventType { get; }
        public Type EventHandlerType { get;}
    }
}

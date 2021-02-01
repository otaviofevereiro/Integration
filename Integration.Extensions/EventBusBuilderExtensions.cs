using Integration.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusBuilderExtensions
    {
        public static EventBusBuilder AddEventHandler<TEvent, TEventHandler>(this EventBusBuilder eventBusBuilder)
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>
        {
            eventBusBuilder.Services.AddTransient<TEventHandler>();
            eventBusBuilder.EventHandlers.Add(new SubscriberInfo(typeof(TEvent), typeof(TEventHandler)));

            return eventBusBuilder;
        }

        public static EventBusBuilder AddEventHandler<TEventHandler>(this EventBusBuilder eventBusBuilder)
                where TEventHandler : class, IRawEventHandler<byte[]>
        {
            eventBusBuilder.Services.AddTransient<TEventHandler>();
            eventBusBuilder.EventHandlers.Add(new SubscriberInfo(typeof(byte[]), typeof(TEventHandler)));

            return eventBusBuilder;
        }
    }
}

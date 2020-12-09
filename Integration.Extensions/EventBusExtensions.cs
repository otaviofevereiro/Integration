using Integration.Core;
using Integration.Extensions;
using Integration.RabbitMq;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services,
                                                     string configurationName,
                                                     Action<EventBusBuilder> eventBusBuilderAction)
        {
            var eventBusBuilder = new EventBusBuilder(configurationName, services);

            eventBusBuilderAction.Invoke(eventBusBuilder);

            return services.AddTransient<SubscriberManager>()
                           .AddSingleton<IEventBusFactory, EventBusFactory>()
                           .AddSingleton(sp => CreateEventBus(eventBusBuilder, sp));
        }

        private static IConfigurableEventBus CreateEventBus(EventBusBuilder eventBusBuilder, IServiceProvider sp)
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(eventBusBuilder.ConfigurationName);
            var eventBusType = section["Type"].ToString();
            var subscriberManager = sp.GetRequiredService<SubscriberManager>();

            foreach (var eventHandlerInfo in eventBusBuilder.EventHandlers)
            {
                subscriberManager.Add(eventHandlerInfo);
            }

            if (eventBusType.Equals("RabbitMq", StringComparison.InvariantCultureIgnoreCase))
                return new RabbitMqEventBus(eventBusBuilder.ConfigurationName, configuration, subscriberManager, sp);
            else
                throw new InvalidOperationException($"The Type '{eventBusType}' of EventBus configuration '{eventBusBuilder.ConfigurationName}' is invalid.");
        }
    }
}

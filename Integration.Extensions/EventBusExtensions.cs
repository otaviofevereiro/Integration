using Integration.Core;
using Integration.Extensions;
using Integration.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services,
                                                     string configurationName,
                                                     Action<EventBusBuilder> eventBusBuilderAction)
        {
            var eventBusBuilder = new EventBusBuilder(configurationName, services);

            eventBusBuilderAction.Invoke(eventBusBuilder);

            return services.AddTransient<SubscriberManager>()
                           .AddTransient<IRabbitMqConnection>(sp => new RabbitMqConnection(eventBusBuilder.ConfigurationName,
                                                                                       sp.GetRequiredService<IConfiguration>()))
                           .AddSingleton<IEventBusFactory, EventBusFactory>()
                           .AddSingleton(sp => CreateEventBus(eventBusBuilder, sp));
        }

        private static IConfigurableEventBus CreateEventBus(EventBusBuilder eventBusBuilder, IServiceProvider sp)
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(eventBusBuilder.ConfigurationName);
            var eventBusType = section["Type"].ToString();
            var subscriberManager = sp.GetRequiredService<SubscriberManager>();
            var connection = sp.GetServices<IRabbitMqConnection>().Single(x => x.Name == eventBusBuilder.ConfigurationName);

            foreach (var eventHandlerInfo in eventBusBuilder.EventHandlers)
            {
                subscriberManager.Add(eventHandlerInfo);
            }

            if (eventBusType.Equals("RabbitMq", StringComparison.InvariantCultureIgnoreCase))
                return new RabbitMqEventBus(eventBusBuilder.ConfigurationName,
                                            configuration,
                                            subscriberManager,
                                            connection,
                                            sp,
                                            sp.GetRequiredService<ILoggerFactory>());
            else
                throw new InvalidOperationException($"The Type '{eventBusType}' of EventBus configuration '{eventBusBuilder.ConfigurationName}' is invalid.");
        }
    }
}

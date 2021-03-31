using Integration.Core;
using Integration.Extensions;
using Integration.RabbitMq;
using Integration.ServiceBus;
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
                           .AddSingleton<IEventBusFactory, EventBusFactory>()
                           .AddSingleton(sp => CreateEventBus(eventBusBuilder, sp))

                           .AddSingleton<IRabbitMqConnection>(sp => new RabbitMqConnection(eventBusBuilder.ConfigurationName,
                                                                                           sp.GetRequiredService<IConfiguration>()))

                           .AddTransient<IServiceBusConnection>(sp => new ServiceBusConnection(eventBusBuilder.ConfigurationName,
                                                                                               sp.GetRequiredService<IConfiguration>(),
                                                                                               sp.GetRequiredService<ILoggerFactory>()));
        }

        private static IConfigurableEventBus CreateEventBus(EventBusBuilder eventBusBuilder, IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(eventBusBuilder.ConfigurationName);
            var eventBusType = section["Type"].ToString();
            var subscriberManager = serviceProvider.GetRequiredService<SubscriberManager>();

            foreach (var eventHandlerInfo in eventBusBuilder.EventHandlers)
            {
                subscriberManager.Add(eventHandlerInfo);
            }

            if (eventBusType.Equals("RabbitMq", StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateRabbitMq(eventBusBuilder, serviceProvider, configuration, subscriberManager);
            }
            else if (eventBusType.Equals("ServiceBus", StringComparison.InvariantCultureIgnoreCase))
            {
                return CreateServiceBus(eventBusBuilder, serviceProvider, subscriberManager);
            }
            else
                throw new InvalidOperationException($"The Type '{eventBusType}' of EventBus configuration '{eventBusBuilder.ConfigurationName}' is invalid.");
        }

        private static IConfigurableEventBus CreateServiceBus(EventBusBuilder eventBusBuilder,
                                                              IServiceProvider serviceProvider,
                                                              SubscriberManager subscriberManager)
        {
            var connection = serviceProvider.GetServices<IServiceBusConnection>()
                                            .Single(x => x.Name == eventBusBuilder.ConfigurationName);

            return new ServiceBusEventBus(eventBusBuilder.ConfigurationName,
                                          connection,
                                          subscriberManager,
                                          serviceProvider,
                                          serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        private static IConfigurableEventBus CreateRabbitMq(EventBusBuilder eventBusBuilder, 
                                                            IServiceProvider serviceProvider, 
                                                            IConfiguration configuration, 
                                                            SubscriberManager subscriberManager)
        {
            var connection = serviceProvider.GetServices<IRabbitMqConnection>()
                                            .Single(x => x.Name == eventBusBuilder.ConfigurationName);

            return new RabbitMqEventBus(eventBusBuilder.ConfigurationName,
                                        configuration,
                                        subscriberManager,
                                        connection,
                                        serviceProvider,
                                        serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}

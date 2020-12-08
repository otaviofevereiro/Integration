using Integration.Core;
using Integration.Extensions;
using Integration.RabbitMq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusExtensions
    {

        //public class EventBusConfigurationInfo
        //{
        //    public EventBusInfo(Action<IConfigurableEventBus> configureEventBus, string name)
        //    {
        //        ConfigureEventBus = configureEventBus;
        //        ConfigurationName = configurationName;
        //    }

        //    public Action<IConfigurableEventBus> ConfigureEventBus { get; }
        //    public string ConfigurationName { get; }
        //}

        public static IServiceCollection AddEventBus(this IServiceCollection services,
                                                     string configurationName,
                                                     Action<IConfigurableEventBus> configureEventBus)
        {
            return services.AddTransient<SubscriberManager>()
                           .AddSingleton<IEventBusFactory, EventBusFactory>()
                           .AddSingleton<IConfigurableEventBus>(sp =>
                           {
                               var configuration = sp.GetRequiredService<IConfiguration>();
                               var section = configuration.GetSection(configurationName);
                               var eventBusType = section["Type"].ToString();

                               IConfigurableEventBus eventBus;

                               if (eventBusType.Equals("RabbitMq", StringComparison.InvariantCultureIgnoreCase))
                                   eventBus = new RabbitMqEventBus(configurationName, sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<SubscriberManager>(), sp);
                               else
                                   throw new InvalidOperationException($"The Type '{eventBusType}' of EventBus configuration '{configurationName}' is invalid.");

                               configureEventBus.Invoke(eventBus);

                               return eventBus;
                           });
        }
    }
}

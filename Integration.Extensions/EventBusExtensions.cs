using Integration.Core;
using Integration.Extensions;
using Integration.RabbitMq;
using Microsoft.Extensions.Configuration;
using System;

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
                           .AddSingleton<IConfigurableEventBus, RabbitMqEventBus>(sp =>
                           {
                               var rabbitMqEventbus = new RabbitMqEventBus(configurationName,
                                                                           sp.GetRequiredService<IConfiguration>(),
                                                                           sp.GetRequiredService<SubscriberManager>(),
                                                                           sp);

                               configureEventBus.Invoke(rabbitMqEventbus);

                               return rabbitMqEventbus;
                           });
        }


    }
}

using Integration.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Integration.Extensions
{
    public class EventBusFactory : IEventBusFactory
    {
        private readonly IServiceProvider serviceProvider;

        public EventBusFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IConfigurableEventBus Create(string configurationName)
        {
            return serviceProvider.GetServices<IConfigurableEventBus>()
                                  .Single(x => x.Name == configurationName);
        }
    }
}

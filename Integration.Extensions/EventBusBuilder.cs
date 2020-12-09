using Integration.Core;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EventBusBuilder
    {
        public EventBusBuilder(string configurationName, IServiceCollection services)
        {
            EventHandlers = new List<SubscriberInfo>();

            ConfigurationName = configurationName;
            Services = services;
        }

        public string ConfigurationName { get; }
        public IServiceCollection Services { get; }
        public List<SubscriberInfo> EventHandlers { get; }
    }
}

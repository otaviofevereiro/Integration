using Integration.Extensions;
using Integration.Hosting.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusBuilderExtesions
    {
        public static EventBusBuilder AddAsHostedService(this EventBusBuilder eventBusBuilder)
        {
            eventBusBuilder.Services.AddTransient<IHostedService, EventBusHostedService>(sp => new EventBusHostedService(eventBusBuilder.ConfigurationName,
                                                                                                                         sp.GetRequiredService<IEventBusFactory>(),
                                                                                                                         sp.GetRequiredService<ILoggerFactory>()));

            return eventBusBuilder;
        }
    }

}

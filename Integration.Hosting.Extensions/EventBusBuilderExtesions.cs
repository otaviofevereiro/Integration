using Integration.Extensions;
using Integration.Hosting.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusBuilderExtesions
    {
        public static EventBusBuilder AddAsHostedService(this EventBusBuilder eventBusBuilder)
        {
            eventBusBuilder.Services.AddTransient<IHostedService, EventBusHostedService>(sp => new EventBusHostedService(eventBusBuilder.ConfigurationName,
                                                                                                                         sp.GetRequiredService<IEventBusFactory>()));

            return eventBusBuilder;
        }
    }

}

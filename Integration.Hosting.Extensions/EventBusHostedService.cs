using Integration.Core;
using Integration.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Hosting.Extensions
{
    public class EventBusHostedService : IHostedService
    {
        private readonly IConfigurableEventBus eventBus;

        public EventBusHostedService(string configurationName, IEventBusFactory eventBusFactory)
        {
            eventBus = eventBusFactory.Create(configurationName);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await eventBus.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //TODO:
            throw new NotImplementedException();
        }
    }
}

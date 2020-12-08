using Integration.Core;
using Integration.Extensions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Test.Receivers
{
    public class AmqpHostedService : IHostedService
    {
        private readonly IConfigurableEventBus eventBus;

        public AmqpHostedService(IEventBusFactory eventBusFactory)
        {
            eventBus = eventBusFactory.Create("Amqp");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await eventBus.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}

using Integration.Core;
using Integration.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Hosting.Extensions
{
    public class EventBusHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<EventBusHostedService> _logger;
        private readonly IConfigurableEventBus eventBus;
        private bool disposedValue;

        public EventBusHostedService(string configurationName, IEventBusFactory eventBusFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EventBusHostedService>();
            eventBus = eventBusFactory.Create(configurationName);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Starting Hosted Service of EventBus {eventBus.Name}");

                await eventBus.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Start Hosted Service of EventBus {eventBus.Name}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Stoping Hosted Service of EventBus {eventBus.Name}");

                await eventBus.Stop(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Stop Hosted Service of EventBus {eventBus.Name}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    eventBus.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;
        private IRabbitMqConnection _connection;

        public RabbitMQHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void EnsureConnection(string name)
        {
            _connection = _serviceProvider.GetServices<IRabbitMqConnection>()
                                          .Single(x => x.Name == name);
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureConnection(context.Registration.Name);

                _connection.Connect();
                _connection.Dispose();

                return await Task.FromResult(HealthCheckResult.Healthy("RabittMQ is Healthy"));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy("RabittMQ is Unhealthy", ex));
            }
        }
    }
}


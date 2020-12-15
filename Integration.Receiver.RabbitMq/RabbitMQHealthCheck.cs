using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IRabbitMqConnection _connection;

        public RabbitMQHealthCheck(IRabbitMqConnection connection)
        {
            _connection = connection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
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


using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public class HealthCheck : IHealthCheck
    {
        private readonly RabbitMqConfiguration configuration;

        public HealthCheck(RabbitMqConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = configuration.ConnectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    //TODO: Add more details
                    return await Task.FromResult(HealthCheckResult.Healthy("RabittMQ is Healthy"));
                }
            }
            catch (Exception ex)
            {
                //TODO: Add more details
                return await Task.FromResult(HealthCheckResult.Unhealthy("RabittMQ is Unhealthy", ex));
            }
        }
    }
}


using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using System;

namespace Integration.RabbitMq
{
    public abstract class RabbitMqReceiver : Receiver<ReceiverConfiguration>
    {
        private readonly ReceiverConfiguration receiverConfig;
        private readonly IConfiguration configuration;

        protected RabbitMqReceiver(IConfiguration configuration)
        {
            this.configuration = configuration;
            receiverConfig = new ReceiverConfiguration();
        }

        private void EnsureConfiguration()
        {
            Configure(receiverConfig);

            if (string.IsNullOrEmpty(receiverConfig.ConfigurationName) ||
                string.IsNullOrEmpty(receiverConfig.QueueConfigurationName))
            {
                //TODO: throw configurationExcecption
                throw new InvalidOperationException();
            }

            receiverConfig.Queue = configuration.GetSection(receiverConfig.QueueConfigurationName) as Queue;
            receiverConfig.RabbitMq = configuration.GetSection(receiverConfig.ConfigurationName) as RabbitMqConfiguration;
        }

        protected void Initialize()
        {
            EnsureConfiguration();

            using (var connection = receiverConfig.RabbitMq.ConnectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(receiverConfig.Queue.Name,
                                       receiverConfig.Queue.Durable,
                                       receiverConfig.Queue.Exclusive,
                                       receiverConfig.Queue.AutoDelete,
                                       receiverConfig.Queue.Arguments);

                    var consumer = new AsyncEventingBasicConsumer(model);

                    consumer.Received += async (message, basicDeliverEventArgs) =>
                    {
                        await OnReceived(message);
                    };
                }
            }
        }
    }
}

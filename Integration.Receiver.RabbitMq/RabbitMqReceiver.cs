using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;

namespace Integration.RabbitMq
{
    public abstract class RabbitMqReceiver : Receiver<RabbitMqConfiguration>
    {
        private readonly RabbitMqConfiguration rabbitMqConfiguration;
        private readonly Queue queue;
        private readonly IConfiguration configuration;

        protected RabbitMqReceiver()
        {
            rabbitMqConfiguration = new RabbitMqConfiguration();
            queue = new Queue();
        }

        protected RabbitMqReceiver(IConfiguration configuration)
        {
            configuration = configuration;
        }

        protected void Initialize()
        {
            using (var connection = rabbitMqConfiguration.ConnectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(queue.Name,
                                       queue.Durable,
                                       queue.Exclusive,
                                       queue.AutoDelete,
                                       queue.Arguments);

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

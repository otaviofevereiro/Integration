using Integration.Core;
using RabbitMQ.Client;

namespace Integration.RabbitMq
{
    public interface IRabbitMqConnection : IEventBusConnection
    {
        internal IModel Model { get; }
    }
}
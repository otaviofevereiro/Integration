using RabbitMQ.Client;
using System;

namespace Integration.RabbitMq
{
    public interface IRabbitMqConnection: IDisposable
    {
        string Name { get; }
        internal IModel Model { get; }

        void Connect();
        void Close();
    }
}
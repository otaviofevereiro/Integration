using Integration.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public sealed class RabbitMqEventBus : EventBus, IConfigurableEventBus
    {
        public readonly ConnectionFactory connectionFactory;
        private readonly IConfiguration configuration;
        private readonly List<Queue> queues;
        private IConnection connection;
        private IModel model;
        //TODO: Add logs

        public RabbitMqEventBus(IConfiguration configuration,
                                SubscriberManager subscriber,
                                IServiceProvider serviceProvider) : base(subscriber, serviceProvider)
        {
            this.configuration = configuration;
            queues = new List<Queue>();
            connectionFactory = new ConnectionFactory
            {
                DispatchConsumersAsync = true
            };
        }

        public string Name { get; private set; }

        public Task Initialize()
        {
            if (connection != null)
                throw new InvalidOperationException("The EventBus already started.");

            connection = connectionFactory.CreateConnection();
            model = connection.CreateModel();

            foreach (var queue in queues)
            {
                if (!string.IsNullOrEmpty(queue.Exchange) &&
                    !string.IsNullOrEmpty(queue.ExchangeType))
                {
                    model.ExchangeDeclare(queue.Exchange, queue.ExchangeType);
                }

                model.QueueDeclare(queue.Name,
                                   queue.Durable,
                                   queue.Exclusive,
                                   queue.AutoDelete,
                                   queue.Arguments);

                Bind(queue);
            }

            return Task.CompletedTask;
        }

        public override async Task Publish(string eventName, object message)
        {
            var queue = GetQueue(eventName);
            var messageJson = JsonConvert.SerializeObject(message, Formatting.None);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            await Task.Run(() => model.BasicPublish(queue.Exchange, queue.RouteKey, model.CreateBasicProperties(), messageBytes));
        }

        internal Task Configure(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));

            if (!string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("The EventBus already configurated");

            Name = name;

            var section = configuration.GetSection(Name);
            var queueSection = configuration.GetSection($"{Name}:Queues");

            section.Bind(connectionFactory);
            queueSection.Bind(queues);

            return Task.CompletedTask;
        }

        protected override Task DoSubscribe(string eventName)
        {
            var queue = GetQueue(eventName);

            Bind(queue);

            return Task.CompletedTask;
        }

        private void Bind(Queue queue)
        {
            model.QueueBind(queue.Name, queue.Exchange, queue.RouteKey);

            var consumer = new AsyncEventingBasicConsumer(model);

            consumer.Received += async (message, basicDeliverEventArgs) =>
            {
                await Notify(basicDeliverEventArgs.RoutingKey, basicDeliverEventArgs.Body.ToArray());
            };

            model.BasicConsume(queue.Name, false, consumer);
        }

        private Queue GetQueue(string eventName)
        {
            //TODO: Add validations
            return queues.Single(x => x.RouteKey == eventName);
        }

        #region Dispose
        private bool disposedValue;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    connection.Dispose();
                    model.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RabbitMqReceiver()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        #endregion Dispose
    }
}

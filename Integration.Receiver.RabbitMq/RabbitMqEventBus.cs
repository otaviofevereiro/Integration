using Integration.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.RabbitMq
{
    public sealed class RabbitMqEventBus : EventBus, IConfigurableEventBus
    {
        public readonly ConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMqConnection _connection;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly List<Queue> _queues;

        public RabbitMqEventBus(string name,
                                IConfiguration configuration,
                                SubscriberManager subscriber,
                                IRabbitMqConnection connection,
                                IServiceProvider serviceProvider,
                                ILoggerFactory loggerFactory) : base(subscriber, serviceProvider, loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));

            Name = name;

            _connection = connection;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<RabbitMqEventBus>();
            _queues = new List<Queue>();
        }

        public string Name { get; }

        public override async Task Publish(string eventName, object @event, CancellationToken cancellationToken = default)
        {
            var queue = GetQueue(eventName);

            _logger.LogDebug($"Found Queue {queue } of Event {eventName}");

            var messageJson = JsonConvert.SerializeObject(@event, Formatting.None);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            _logger.LogDebug($"Publishing a new message on Exchange {queue.Exchange}, RouteKey {queue.RouteKey}");

            await Task.Run(() => _connection.Model.BasicPublish(queue.Exchange, queue.RouteKey, _connection.Model.CreateBasicProperties(), messageBytes), cancellationToken);
        }

        public Task Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting RabbitMQ EventBus {Name}");

            _connection.Connect();

            var queueSection = _configuration.GetSection($"{Name}:Queues");
            queueSection.Bind(_queues);

            foreach (var queue in _queues)
            {
                if (!string.IsNullOrEmpty(queue.Exchange) &&
                    !string.IsNullOrEmpty(queue.ExchangeType))
                {
                    _logger.LogInformation($"Declare Exchange {queue.Exchange} {queue.ExchangeType}");

                    _connection.Model.ExchangeDeclare(queue.Exchange, queue.ExchangeType);
                }

                _logger.LogInformation($"Declare Queue {queue.Name}");

                _connection.Model.QueueDeclare(queue.Name,
                                               queue.Durable,
                                               queue.Exclusive,
                                               queue.AutoDelete,
                                               queue.Arguments);
            }

            foreach (var handler in _subscriber.Handlers)
            {
                DoSubscribe(handler.Key);
            }

            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stoping RabbitMQ EventBus {Name}");

            _connection.Close();

            return Task.CompletedTask;
        }

        protected override void DoSubscribe(string eventName)
        {
            var queue = GetQueue(eventName);

            Bind(queue);
        }

        private void Bind(Queue queue)
        {
            _connection.Model.QueueBind(queue.Name, queue.Exchange, queue.RouteKey);

            _logger.LogInformation($"Binded Queue {queue.Name}, Exchange {queue.Exchange} and RouteKey {queue.RouteKey}");

            var consumer = new AsyncEventingBasicConsumer(_connection.Model);

            consumer.Received += async (sender, eventsArgs) =>
            {
                try
                {
                    _logger.LogDebug($"A new message was received from {Name} with RoutingKey {eventsArgs.RoutingKey}.");
                    _logger.LogDebug($"Notifying handlers...");

                    using (var eventContext = new EventContext(eventsArgs.RoutingKey, eventsArgs.Body))
                    {
                        await Notify(eventContext);

                        if (eventContext.IsAllComplete)
                            _connection.Model.BasicAck(eventsArgs.DeliveryTag, multiple: false);
                        //else if (eventContext.HasDeadLetter)
                            //TODO: send to dead letter
                    }

                    _logger.LogDebug($"Message {eventsArgs.DeliveryTag} was Ack...");

                }
                catch (Exception ex)
                {
                    _connection.Model.BasicNack(eventsArgs.DeliveryTag, multiple: false, requeue: true);
                    _logger.LogError(ex, $"Message {eventsArgs.DeliveryTag} was Nack...");

                    throw;
                }
            };

            _connection.Model.BasicConsume(queue.Name, autoAck: false, consumer);
            _logger.LogInformation($"Added consumer to Queue {queue.Name}");
        }

        private Queue GetQueue(string eventName)
        {
            var queue = _queues.SingleOrDefault(x => x.RouteKey == eventName);

            if (queue == null)
                throw new InvalidOperationException($"Not Queue found with RouteKey '{eventName}'");

            return queue;
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
                    _connection.Dispose();
                }
                disposedValue = true;
            }
        }

        public override Task Publish(string eventName, IEnumerable<object> events, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion Dispose
    }
}

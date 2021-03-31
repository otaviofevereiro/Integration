using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Integration.ServiceBus
{
    public class ServiceBusConnection : IServiceBusConnection
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusConnection> _logger;
        private ConcurrentDictionary<string, IReceiverClient> _receivers = new();
        private ConcurrentDictionary<string, ISenderClient> _senders = new();

        private bool disposedValue;
        public ServiceBusConnection(string name,
                                    IConfiguration configuration,
                                    ILoggerFactory loggerFactory)
        {
            Name = name;

            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<ServiceBusConnection>();
        }

        ~ServiceBusConnection()
        {
            Dispose(disposing: false);
        }

        public string Name { get; }

        public async Task Close()
        {
            var sendersCloseTasks = _senders.Select(x => CloseClientEntity(x.Value));
            var receiversCloseTasks = _receivers.Select(x => CloseClientEntity(x.Value));
            var closeTasks = sendersCloseTasks.Concat(receiversCloseTasks);

            await Task.WhenAll(closeTasks.ToArray());
        }

        public Task Connect()
        {
            var queues = new List<Queue>();

            var section = _configuration.GetSection(Name);
            var queuesSection = section.GetSection($"Queues");

            section.Bind(queuesSection);

            foreach (var queue in queues)
            {
                if (!string.IsNullOrEmpty(queue.QueueName))
                {
                    var connectionString = new ServiceBusConnectionStringBuilder();
                    section.Bind(connectionString);
                    connectionString.EntityPath = queue.QueueName;

                    var queueClient = new QueueClient(connectionString);

                    EnsureReceiverClient(queuesSection, queueClient);

                    _logger.LogInformation($"You will receive messages from event '{queue.EventName}' from queue '{queue.QueueName}'");
                    _receivers.TryAdd(queue.EventName, queueClient);

                    if (string.IsNullOrEmpty(queue.Topic))
                    {
                        _logger.LogInformation($"You will send messages from event '{queue.EventName}' to queue '{queue.QueueName}'");
                        _senders.TryAdd(queue.EventName, queueClient);
                    }
                }

                if (!string.IsNullOrEmpty(queue.Topic))
                {
                    var connectionString = new ServiceBusConnectionStringBuilder();
                    section.Bind(connectionString);
                    connectionString.EntityPath = queue.Topic;

                    _logger.LogInformation($"You will send messages from event '{queue.EventName}' to topic '{queue.Topic}'");

                    var topicClient = new TopicClient(connectionString);

                    topicClient.OperationTimeout = queuesSection.GetValue("OperationTimeout", TimeSpan.FromMinutes(3));

                    _senders.TryAdd(queue.EventName, topicClient);

                    if (!string.IsNullOrEmpty(queue.Subscription))
                    {
                        var subscriptionClient = new SubscriptionClient(connectionString, queue.Subscription);

                        EnsureReceiverClient(queuesSection, subscriptionClient);

                        bool sucessAdded = _receivers.TryAdd(queue.EventName, subscriptionClient);

                        if (!sucessAdded)
                            throw new InvalidOperationException($"You cannot provide both QueueName and Subscribtion for the same queue on '{Name}' configuration");

                        _logger.LogInformation($"You will receive messages from event '{queue.EventName}' from subscription '{queue.Subscription}'");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IReceiverClient GetReceiverClient(string eventName)
        {
            if (_receivers.TryGetValue(eventName, out IReceiverClient receiver))
                return receiver;

            throw new InvalidOperationException($"I was not possible find receiver client. Not subscription of event '{eventName}' on eventbus '{Name}'");
        }

        public ISenderClient GetSenderClient(string eventName)
        {
            if (_senders.TryGetValue(eventName, out ISenderClient sender))
                return sender;

            throw new InvalidOperationException($"I was not possible find sender client. Not subscription of event '{eventName}' on eventbus '{Name}'");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close().GetAwaiter().GetResult();
                }

                _receivers = null;
                _senders = null;
                disposedValue = true;
            }
        }

        private Task CloseClientEntity(IClientEntity clientEntity)
        {
            if (clientEntity.IsClosedOrClosing)
                return Task.CompletedTask;
            else
                return clientEntity.CloseAsync();
        }

        private void EnsureReceiverClient(IConfigurationSection queuesSection, IReceiverClient client)
        {
            client.PrefetchCount = queuesSection.GetValue("PrefetchCount", 0);
            client.OperationTimeout = queuesSection.GetValue("OperationTimeout", TimeSpan.FromMinutes(3));
        }
    }
}

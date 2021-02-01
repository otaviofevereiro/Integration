using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;

namespace Integration.ServiceBus
{
    public class ServiceBusConnection : IServiceBusConnection
    {
        private readonly IConfiguration _configuration;
        private ConcurrentDictionary<string, IReceiverClient> _receivers = new();
        private ConcurrentDictionary<string, ISenderClient> _senders = new();

        private bool disposedValue;
        public ServiceBusConnection(string name,
                                 IConfiguration configuration)
        {
            Name = name;
            _configuration = configuration;


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
                var connectionString = new ServiceBusConnectionStringBuilder();
                section.Bind(connectionString);

                connectionString.EntityPath = queue.QueueName ?? queue.Topic;

                if (!string.IsNullOrEmpty(queue.QueueName))
                {
                    var queueClient = new QueueClient(connectionString);

                    _receivers.TryAdd(queue.QueueName, queueClient);
                    _senders.TryAdd(queue.QueueName, queueClient);
                }
                else if (!string.IsNullOrEmpty(queue.Topic))
                {
                    if (!string.IsNullOrEmpty(queue.Subscription))
                        _receivers.TryAdd(queue.Topic, new SubscriptionClient(connectionString, queue.Subscription));

                    _senders.TryAdd(queue.Topic, new TopicClient(connectionString));
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
            return _receivers.GetValueOrDefault(eventName);
        }

        public ISenderClient GetSenderClient(string eventName)
        {
            return _senders.GetValueOrDefault(eventName);
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
    }
}

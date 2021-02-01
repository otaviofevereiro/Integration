using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Integration.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Integration.ServiceBus
{
    public class ServiceBusEventBus : EventBus, IConfigurableEventBus
    {
        private readonly IServiceBusConnection _connection;
        private bool disposedValue;

        public ServiceBusEventBus(string configurationName,
                                  IServiceBusConnection connection,
                                  SubscriberManager subscriber,
                                  IServiceProvider serviceProvider,
                                  ILoggerFactory loggerFactory)
            : base(subscriber, serviceProvider, loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
                throw new ArgumentException($"'{nameof(configurationName)}' cannot be null or whitespace", nameof(configurationName));

            Name = configurationName;
            _connection = connection;
        }

        public string Name { get; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override async Task Publish(string eventName, object @event, CancellationToken cancellationToken = default)
        {
            var senderClient = _connection.GetSenderClient(eventName);
            var message = GetMessage(@event);

            await senderClient.SendAsync(message);
        }

        public override async Task Publish(string eventName, IEnumerable<object> events, CancellationToken cancellationToken = default)
        {
            var senderClient = _connection.GetSenderClient(eventName);
            var messages = events.Select(@event => GetMessage(@event))
                                 .ToList();

            await senderClient.SendAsync(messages);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await _connection.Connect();
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _connection.Close();
        }

        protected virtual void Dispose(bool disposing)
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

        protected override void DoSubscribe(string eventName)
        {
            var receiverClient = _connection.GetReceiverClient(eventName);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 100,//TODO:
                AutoComplete = false,
            };

            receiverClient.RegisterMessageHandler(async (Message message, CancellationToken token) =>
            {
                using (var eventContext = new EventContext(eventName, message.Body))
                {
                    await Notify(eventContext);
                }
            },
            messageHandlerOptions);
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs eventArgs)
        {
            //TODO:
        }

        private Message GetMessage(object @event)
        {
            var messageJson = JsonConvert.SerializeObject(@event, Formatting.None);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            return new Message(messageBytes);
        }
    }
}

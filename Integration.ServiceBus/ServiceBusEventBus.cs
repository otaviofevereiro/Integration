using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Integration.Core;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Integration.ServiceBus
{
    public class ServiceBusEventBus : EventBus, IConfigurableEventBus
    {
        private readonly IServiceBusConnection _connection;
        private readonly IConfiguration _configuration;
        private bool disposedValue;

        public ServiceBusEventBus(string configurationName,
                                  IServiceBusConnection connection,
                                  SubscriberManager subscriber,
                                  IServiceProvider serviceProvider,
                                  ILoggerFactory loggerFactory,
                                  IConfiguration configuration)
            : base(subscriber, serviceProvider, loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
                throw new ArgumentException($"'{nameof(configurationName)}' cannot be null or whitespace", nameof(configurationName));

            Name = configurationName;
            _connection = connection;
            _configuration = configuration;

            //TODO: Add logs
        }

        public string Name { get; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override async Task Publish<TEvent>(string eventName, TEvent @event, IDictionary<string, object> properties = null, CancellationToken cancellationToken = default)
        {
            var senderClient = _connection.GetSenderClient(eventName);
            var message = GetPublishMessage(@event);

            await senderClient.SendAsync(message);
        }

        public override async Task Publish<TEvent>(string eventName, IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        {
            var senderClient = _connection.GetSenderClient(eventName);
            var messages = events.Select(@event => GetPublishMessage(@event))
                                 .ToList();

            await senderClient.SendAsync(messages);
        }

        public override async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            await Publish(typeof(TEvent).Name, @event, @event.UserProperties, cancellationToken);
        }

        public override async Task Publish<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        {
            var senderClient = _connection.GetSenderClient(typeof(TEvent).Name);
            var messages = events.Select(@event => GetPublishMessage(@event, @event.UserProperties))
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
            var messageHandlerOptions = GetMessageHandlerOptions();

            receiverClient.RegisterMessageHandler(async (Message message, CancellationToken token) =>
            {
                bool complete = await Notify(message.MessageId, eventName, message.Body, message.UserProperties);

                if (complete)
                    await receiverClient.CompleteAsync(message.MessageId);
                else
                    await receiverClient.DeadLetterAsync(message.MessageId);
            },
            messageHandlerOptions);
        }

        private MessageHandlerOptions GetMessageHandlerOptions()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler);

            _configuration.GetSection(Name)
                          .GetSection(nameof(MessageHandlerOptions))
                          .Bind(messageHandlerOptions);

            messageHandlerOptions.AutoComplete = false;

            return messageHandlerOptions;
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs eventArgs)
        {
            //TODO:
        }

        private Message GetPublishMessage(object @event, IDictionary<string, object> properties = null)
        {
            var messageJson = JsonConvert.SerializeObject(@event, Formatting.None);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var message = new Message(messageBytes);

            foreach (var property in properties)
            {
                message.UserProperties.Add(property.Key, property.Value);
            }

            return message;
        }
    }
}

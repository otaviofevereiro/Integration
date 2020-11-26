using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Integration.RabbitMq
{
    public class RabbitMqConfiguration : IReceiverConfiguration
    {
        internal RabbitMqConfiguration()
        {
            ConnectionFactory = new ConnectionFactory();
        }

        public IDictionary<string, object> ClientProperties
        {
            get { return ConnectionFactory.ClientProperties; }
            init { ConnectionFactory.ClientProperties = value; }
        }

        public string ClientProvidedName
        {
            get { return ConnectionFactory.ClientProvidedName; }
            init { ConnectionFactory.ClientProvidedName = value; }
        }

        public string ConfigurationName { get; private set; }
        public ConnectionFactory ConnectionFactory { get; }

        public TimeSpan ContinuationTimeout
        {
            get { return ConnectionFactory.ContinuationTimeout; }
            init { ConnectionFactory.ContinuationTimeout = value; }
        }

        public TimeSpan HandshakeContinuationTimeout
        {
            get { return ConnectionFactory.HandshakeContinuationTimeout; }
            init { ConnectionFactory.HandshakeContinuationTimeout = value; }
        }

        public string Password
        {
            get { return ConnectionFactory.Password; }
            init { ConnectionFactory.Password = value; }
        }

        public string QueueConfigurationName { get; private set; }

        public ushort RequestedChannelMax
        {
            get { return ConnectionFactory.RequestedChannelMax; }
            init { ConnectionFactory.RequestedChannelMax = value; }
        }
        public uint RequestedFrameMax
        {
            get { return ConnectionFactory.RequestedFrameMax; }
            init { ConnectionFactory.RequestedFrameMax = value; }
        }

        public TimeSpan RequestedHeartbeat
        {
            get { return ConnectionFactory.RequestedHeartbeat; }
            init { ConnectionFactory.RequestedHeartbeat = value; }
        }

        public Uri Uri
        {
            get { return ConnectionFactory.Uri; }
            init { ConnectionFactory.Uri = value; }
        }

        public bool UseBackgroundThreadsForIO
        {
            get { return ConnectionFactory.UseBackgroundThreadsForIO; }
            init { ConnectionFactory.UseBackgroundThreadsForIO = value; }
        }

        public string UserName
        {
            get { return ConnectionFactory.UserName; }
            init { ConnectionFactory.UserName = value; }
        }

        public string VirtualHost
        {
            get { return ConnectionFactory.VirtualHost; }
            init { ConnectionFactory.VirtualHost = value; }
        }
        public RabbitMqConfiguration WithQueueConfiguration(string queueConfigurationName)
        {
            QueueConfigurationName = queueConfigurationName;

            return this;
        }

        public RabbitMqConfiguration WithRabbitMqConfiguration(string configurationName)
        {
            ConfigurationName = configurationName;

            return this;
        }
    }
}

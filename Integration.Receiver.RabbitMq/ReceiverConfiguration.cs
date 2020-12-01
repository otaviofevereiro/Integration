namespace Integration.RabbitMq
{
    public class ReceiverConfiguration : IReceiverConfiguration
    {
        public string ConfigurationName { get; private set; }
        public string QueueConfigurationName { get; private set; }
        internal RabbitMqConfiguration RabbitMq { get; set; }
        internal Queue Queue { get; set; }

        public ReceiverConfiguration WithQueueConfiguration(string queueConfigurationName)
        {
            QueueConfigurationName = queueConfigurationName;

            return this;
        }

        public ReceiverConfiguration WithRabbitMqConfiguration(string configurationName)
        {
            ConfigurationName = configurationName;

            return this;
        }
    }
}

using Integration.RabbitMq;
using Microsoft.Extensions.Configuration;

namespace Integration.Test.Receivers
{
    public class PeopleReceiver : RabbitMqReceiver
    {
        public PeopleReceiver(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void Configure(ReceiverConfiguration configuration)
        {
            configuration.WithRabbitMqConfiguration("RabbitMq")
                         .WithQueueConfiguration("PeopleRabbitMq");
        }
    }
}

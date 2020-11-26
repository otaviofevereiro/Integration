using Integration.RabbitMq;

namespace Integration.Test.Receivers
{
    public class PeopleReceiver : RabbitMqReceiver
    {
        protected override void Configure(RabbitMqConfiguration configuration)
        {
            configuration.WithRabbitMqConfiguration("RabbitMq")
                         .WithQueueConfiguration("PeopleRabbitMq");
        }
    }
}

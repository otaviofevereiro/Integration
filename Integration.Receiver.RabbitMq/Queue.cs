using System.Collections.Generic;

namespace Integration.RabbitMq
{
    public class Queue
    {
        public string Exchange { get; init; }
        public string ExchangeType { get; init; }
        public string RouteKey { get; init; }
        public string Name { get; init; }
        public bool Durable { get; init; } = true;
        public bool Exclusive { get; init; } = false;
        public bool AutoDelete { get; init; } = false;
        public IDictionary<string, object> Arguments { get; init; }
    }
}

using System.Collections.Generic;

namespace Integration.RabbitMq
{
    public class Queue
    {
        public string Name { get; init; }
        public bool Durable { get; init; } = true;
        public bool Exclusive { get; init; }
        public bool AutoDelete { get; init; }
        public IDictionary<string, object> Arguments { get; init; }
    }
}

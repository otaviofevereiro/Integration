using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IPublisher
    {
        Task Publish<TEvent>(string eventName, TEvent @event, IDictionary<string, object> properties = null,
            CancellationToken cancellationToken = default);

        Task Publish<TEvent>(string eventName, IEnumerable<TEvent> events, CancellationToken cancellationToken = default);

        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : Event;

        Task Publish<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
            where TEvent : Event;

    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface IPublisher
    {
        public Task Publish(string eventName, object @event, CancellationToken cancellationToken = default);
    }
}

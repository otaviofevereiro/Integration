using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface ISubscriber
    {
        public Task Subscribe<TEvent, TEventHandler>(CancellationToken cancellationToken= default)
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>;
    }
}

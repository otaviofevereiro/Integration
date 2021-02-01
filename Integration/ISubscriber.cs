using System.Threading;
using System.Threading.Tasks;

namespace Integration.Core
{
    public interface ISubscriber
    {
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : class, IEventHandler<TEvent>;
    }
}
